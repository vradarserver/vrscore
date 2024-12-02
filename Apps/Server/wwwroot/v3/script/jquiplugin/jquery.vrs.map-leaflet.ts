﻿/**
 * @license Copyright © 2018 onwards, Andrew Whewell
 * All rights reserved.
 *
 * Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 *    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 *    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 *    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
/**
 * @fileoverview A jQuery UI plugin that wraps Leaflet maps.
 */

namespace VRS
{
    /*
     * Backwards compatible global options. These are not used, they are here so that custom scripts that
     * rely on them won't crash.
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapGoogleMapHttpUrl = VRS.globalOptions.mapGoogleMapHttpUrl || '';
    VRS.globalOptions.mapGoogleMapHttpsUrl = VRS.globalOptions.mapGoogleMapHttpsUrl || '';
    VRS.globalOptions.mapGoogleMapTimeout = VRS.globalOptions.mapGoogleMapTimeout || 30000;
    VRS.globalOptions.mapGoogleMapUseHttps = VRS.globalOptions.mapGoogleMapUseHttps !== undefined ? VRS.globalOptions.mapGoogleMapUseHttps : true;
    VRS.globalOptions.mapShowStreetView = VRS.globalOptions.mapShowStreetView !== undefined ? VRS.globalOptions.mapShowStreetView : false;
    VRS.globalOptions.mapShowHighContrastStyle = VRS.globalOptions.mapShowHighContrastStyle !== undefined ? VRS.globalOptions.mapShowHighContrastStyle : true;
    VRS.globalOptions.mapHighContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle !== undefined ? VRS.globalOptions.mapHighContrastMapStyle : [];

    /*
     * These options are shared with Google Maps. They should probably be moved out of both plugin modules to somewhere common.
     */
    VRS.globalOptions.mapScrollWheelActive = VRS.globalOptions.mapScrollWheelActive !== undefined ? VRS.globalOptions.mapScrollWheelActive : true;
    VRS.globalOptions.mapDraggable = VRS.globalOptions.mapDraggable !== undefined ? VRS.globalOptions.mapDraggable : true;
    VRS.globalOptions.mapShowPointsOfInterest = VRS.globalOptions.mapShowPointsOfInterest !== undefined ? VRS.globalOptions.mapShowPointsOfInterest : false;
    VRS.globalOptions.mapShowScaleControl = VRS.globalOptions.mapShowScaleControl !== undefined ? VRS.globalOptions.mapShowScaleControl : true;

    /*
     * These options are only used by Leaflet.
     */
    VRS.globalOptions.mapLeafletNoWrap = VRS.globalOptions.mapLeafletNoWrap !== undefined ? VRS.globalOptions.mapLeafletNoWrap : true;

    export type LeafletControlPosition = 'topleft' | 'topright' | 'bottomleft' | 'bottomright';

    export class LeafletUtilities
    {
        fromLeafletLatLng(latLng: L.LatLng): ILatLng
        {
            return latLng;
        }

        toLeafletLatLng(latLng: ILatLng, map: L.Map): L.LatLng
        {
            if(latLng instanceof L.LatLng) {
                return latLng;
            } else if(latLng) {
                return map ? map.wrapLatLng([ latLng.lat, latLng.lng ]) : new L.LatLng(latLng.lat, latLng.lng);
            }

            return null;
        }

        fromLeafletLatLngArray(latLngArray: L.LatLng[]): ILatLng[]
        {
            return latLngArray;
        }

        toLeafletLatLngArray(latLngArray: ILatLng[], map: L.Map): L.LatLng[]
        {
            latLngArray = latLngArray || [];

            var result = [];
            var len = latLngArray.length;
            for(var i = 0;i < len;++i) {
                result.push(this.toLeafletLatLng(latLngArray[i], map));
            }

            return result;
        }

        fromLeafletLatLngBounds(bounds: L.LatLngBounds): IBounds
        {
            if(!bounds) {
                return null;
            }
            return {
                tlLat: bounds.getNorth(),
                tlLng: bounds.getWest(),
                brLat: bounds.getSouth(),
                brLng: bounds.getEast()
            };
        }

        toLeaftletLatLngBounds(bounds: IBounds, map: L.Map): L.LatLngBounds
        {
            if(!bounds) {
                return null;
            }
            return map
                   ? map.wrapLatLngBounds(new L.LatLngBounds([ bounds.brLat, bounds.tlLng ], [ bounds.tlLat, bounds.brLng ]))
                   : new L.LatLngBounds([ bounds.brLat, bounds.tlLng ], [ bounds.tlLat, bounds.brLng ]);
        }

        fromLeafletIcon(icon: L.Icon|L.DivIcon): IMapIcon
        {
            // For now I'm just supporting Icon objects
            if(icon === null || icon === undefined) {
                return null;
            }

            return new MapIcon(
                icon.options.iconUrl,
                VRS.leafletUtilities.fromLeafletSize(icon.options.iconSize),
                VRS.leafletUtilities.fromLeafletPoint(icon.options.iconAnchor),
                null,
                null
            );
        }

        toLeafletIcon(icon: string|IMapIcon): L.Icon
        {
            if(typeof icon === 'string') {
                return null;
            }

            return L.icon({
                iconUrl: icon.url,
                iconSize: VRS.leafletUtilities.toLeafletSize(icon.size),
                iconAnchor: VRS.leafletUtilities.toLeafletPoint(icon.anchor)
            });
        }

        fromLeafletContent(content: L.Content): string
        {
            if(content === null || content === undefined) {
                return null;
            } else {
                if(typeof content === "string") {
                    return content;
                }
                return (<HTMLElement>content).innerText;
            }
        }

        fromLeafletSize(size: L.PointExpression): ISize
        {
            if(size === null || size === undefined) {
                return null;
            }
            if(size instanceof L.Point) {
                return {
                    width: size.x,
                    height: size.y
                };
            }
            return {
                width:  (<L.PointTuple>size)[0],
                height: (<L.PointTuple>size)[1]
            };
        }

        toLeafletSize(size: ISize): L.Point
        {
            if(size === null || size === undefined) {
                return null;
            }
            return L.point(size.width, size.height);
        }

        fromLeafletPoint(point: L.PointExpression): IPoint
        {
            if(point === null || point === undefined) {
                return null;
            }
            if(point instanceof L.Point) {
                return point;
            }
            return {
                x:  (<L.PointTuple>point)[0],
                y: (<L.PointTuple>point)[1]
            };
        }

        toLeafletPoint(point: IPoint): L.Point
        {
            if(point === null || point === undefined) {
                return null;
            }
            if(point instanceof L.Point) {
                return point;
            }
            return L.point(point.x, point.y);
        }

        fromLeafletMapPosition(mapPosition: LeafletControlPosition): VRS.MapPositionEnum
        {
            switch(mapPosition || '') {
                case 'topleft':     return VRS.MapPosition.TopLeft;
                case 'bottomleft':  return VRS.MapPosition.BottomLeft;
                case 'bottomright': return VRS.MapPosition.BottomRight;
                default:            return VRS.MapPosition.TopRight;
            }
        }

        toLeafletMapPosition(mapPosition: VRS.MapPositionEnum): LeafletControlPosition
        {
            switch(mapPosition || VRS.MapPosition.TopRight) {
                case VRS.MapPosition.BottomCentre:
                case VRS.MapPosition.BottomLeft:
                case VRS.MapPosition.LeftBottom:
                case VRS.MapPosition.LeftCentre:
                    return 'bottomleft';
                case VRS.MapPosition.BottomRight:
                case VRS.MapPosition.RightBottom:
                case VRS.MapPosition.RightCentre:
                    return 'bottomright';
                case VRS.MapPosition.LeftTop:
                case VRS.MapPosition.TopCentre:
                case VRS.MapPosition.TopLeft:
                    return 'topleft';
                default:
                    return 'topright';
            }
        }

        toLeafletMapMarker(mapMarker: IMapMarker) : L.Marker
        {
            var wrapper = <MapMarker>mapMarker;
            return wrapper ? wrapper.marker : null;
        }

        toLeafletMapMarkers(mapMarkers: IMapMarker[]) : L.Marker[]
        {
            var result : L.Marker[] = [];
            var len = mapMarkers ? mapMarkers.length : 0;
            for(var i = 0;i < len;++i) {
                result.push(this.toLeafletMapMarker(mapMarkers[i]));
            }

            return result;
        }
    }
    export var leafletUtilities = new LeafletUtilities();

    /*
     * jQueryUIHelper
     */
    export var jQueryUIHelper: JQueryUIHelper = VRS.jQueryUIHelper || {};
    jQueryUIHelper.getMapPlugin = (jQueryElement: JQuery) : IMap =>
    {
        return <IMap>jQueryElement.data('vrsVrsMap');
    }
    jQueryUIHelper.getMapOptions = (overrides: IMapOptions) : IMapOptions =>
    {
        return $.extend({
            // Google Maps options, these are ignored
            key:                null,                                   // Unused
            version:            '',                                     // Unused
            sensor:             false,                                  // Unused
            libraries:          [],                                     // Unused
            loadMarkerWithLabel:false,                                  // Unused
            loadMarkerCluster:  false,                                  // Unused

            // Map options
            openOnCreate:       true,                                   // Open the map when the widget is created, if false then the code that creates the map has to call open() itself.
            waitUntilReady:     true,                                   // If true then the widget does not call afterOpen until after the map has completely loaded. If this is false then calling getBounds (and perhaps other calls) may fail until the map has loaded.
            zoom:               12,                                     // The zoom level to open with
            center:             { lat: 51.5, lng: -0.125 },             // The location to centre the map on
            showMapTypeControl: true,                                   // True to show the map type control, false to hide it.
            mapTypeId:          VRS.MapType.Hybrid,                     // The map type to start with
            streetViewControl:  VRS.globalOptions.mapShowStreetView,    // Whether to show Street View or not
            scrollwheel:        VRS.globalOptions.mapScrollWheelActive, // Whether the scrollwheel zooms the map
            scaleControl:       VRS.globalOptions.mapShowScaleControl,  // Whether to show the scale control or not.
            draggable:          VRS.globalOptions.mapDraggable,         // Whether the map is draggable
            controlStyle:       VRS.MapControlStyle.Default,            // The style of map control to display
            controlPosition:    undefined,                              // Where the map control should be placed
            pointsOfInterest:   VRS.globalOptions.mapShowPointsOfInterest, // Whether to show Google Maps' points of interest
            showHighContrast:   VRS.globalOptions.mapShowHighContrastStyle, // Whether to show the custom high-contrast map style or not.

            // Custom map open options
            mapControls:        [],                                     // Controls to add to the map after it has been opened.

            // Callbacks
            afterCreate:        null,                                   // Called after the map has been created but before it has been opened
            afterOpen:          null,                                   // Called after the map has been opened

            // Persistence options
            name:               'default',                              // The name to use to distinguish the state settings from those of other maps
            useStateOnOpen:     false,                                  // Load and apply state when opening the map
            autoSaveState:      false,                                  // Automatically save state whenever any state variable is changed by the user
            useServerDefaults:  false,                                  // Always use the server-supplied configuration settings rather than those in options.

            __nop:      null
        }, overrides);
    }

    /**
     * An abstracted wrapper around an object that represents a leaflet marker.
     */
    class MapMarker implements IMapMarker
    {
        id: string|number;
        mapPlugin: MapPlugin;
        marker: L.Marker;
        map: L.Map;
        mapIcon: IMapIcon;
        zIndex: number;
        isMarkerWithLabel: boolean;
        tag: any;
        visible: boolean;
        labelTooltip: L.Tooltip;

        private _eventsHooked = false;

        /**
         * Creates a new object.
         * @param {string|number}       id                  The identifier of the marker.
         * @param {MapPlugin}           mapPlugin           The map plugin that owns this marker.
         * @param {L.Map}               nativeMap           The map that this marker will be (or is) attached to.
         * @param {L.Marker}            nativeMarker        The native map marker handle to wrap.
         * @param {L.MarkerOptions}     markerOptions       The options used when creating the marker.
         * @param {IMapMarkerSettings}  userOptions         The options passed for the creation of the marker.
        */
        constructor(id: string|number, mapPlugin: MapPlugin, map: L.Map, nativeMarker: L.Marker, markerOptions: L.MarkerOptions, userOptions: IMapMarkerSettings)
        {
            this.id = id;
            this.mapPlugin = mapPlugin;
            this.map = map;
            this.marker = nativeMarker;
            this.mapIcon = VRS.leafletUtilities.fromLeafletIcon(markerOptions.icon);
            this.zIndex = markerOptions.zIndexOffset;
            this.isMarkerWithLabel = !!userOptions.useMarkerWithLabel;
            this.tag = userOptions.tag;
            this.visible = !!userOptions.visible;

            if(this.isMarkerWithLabel) {
                this.labelTooltip = new L.Tooltip({
                    permanent: true,
                    className: userOptions.mwlLabelClass,
                    direction: 'bottom',
                    pane: 'shadowPane'
                });
                this.labelTooltip.setLatLng(this.marker.getLatLng());
            }

            this.hookEvents(true);
        }

        destroy()
        {
            if(this.labelTooltip) {
                this.setLabelVisible(false);
                this.labelTooltip = null;
            }

            this.hookEvents(false);
            this.setVisible(false);
            this.mapPlugin = null;
            this.marker = null;
            this.map = null;
            this.tag = null;
        }

        hookEvents(hook: boolean)
        {
            if(this._eventsHooked !== hook) {
                this._eventsHooked = hook;

                if(hook) this.marker.on ('click', this._marker_clicked, this);
                else     this.marker.off('click', this._marker_clicked, this);

                if(hook) this.marker.on ('dragend', this._marker_dragged, this);
                else     this.marker.off('dragend', this._marker_dragged, this);
            }
        }

        private _marker_clicked(e: Event)
        {
            this.mapPlugin.raiseMarkerClicked(this.id);
        }

        private _marker_dragged(e: L.DragEndEvent)
        {
            this.mapPlugin.raiseMarkerDragged(this.id);
        }

        /**
         * Returns true if the marker can be dragged.
         */
        getDraggable() : boolean
        {
            return this.marker.dragging.enabled();
        }

        /**
         * Sets a value indicating whether the marker can be dragged.
         */
        setDraggable(draggable: boolean)
        {
            if(draggable) {
                this.marker.dragging.enable();
            } else {
                this.marker.dragging.disable();
            }
        }

        /**
         * Returns the icon for the marker.
         */
        getIcon() : IMapIcon
        {
            return this.mapIcon;
        }

        /**
         * Sets the icon for the marker.
         */
        setIcon(icon: IMapIcon)
        {
            this.marker.setIcon(VRS.leafletUtilities.toLeafletIcon(icon));
            this.mapIcon = icon;
        }

        /**
         * Gets the coordinates of the marker.
         */
        getPosition() : ILatLng
        {
            return VRS.leafletUtilities.fromLeafletLatLng(this.marker.getLatLng());
        }

        /**
         * Sets the coordinates for the marker.
         */
        setPosition(position: ILatLng)
        {
            this.marker.setLatLng(VRS.leafletUtilities.toLeafletLatLng(position, this.map));
            if(this.labelTooltip) {
                this.labelTooltip.setLatLng(this.marker.getLatLng());
            }
        }

        /**
         * Gets the tooltip for the marker.
         */
        getTooltip() : string
        {
            var icon = this.marker.getElement();
            return icon ? icon.title : null;
        }

        /**
         * Sets the tooltip for the marker.
         */
        setTooltip(text: string)
        {
            var icon = this.marker.getElement();
            if(icon) {
                icon.title = text;
            }
        }

        /**
         * Gets a value indicating that the marker is visible.
         */
        getVisible() : boolean
        {
            return this.visible;
        }

        /**
         * Sets a value indicating whether the marker is visible.
         */
        setVisible(visible: boolean)
        {
            if(visible !== this.getVisible()) {
                if(visible) {
                    this.marker.addTo(this.map);
                } else {
                    this.marker.removeFrom(this.map);
                }
                this.visible = visible;
            }
        }

        /**
         * Gets the z-index of the marker.
         */
        getZIndex() : number
        {
            return this.zIndex;
        }

        /**
         * Sets the z-index of the marker.
         */
        setZIndex(zIndex: number)
        {
            this.marker.setZIndexOffset(zIndex);
            this.zIndex = zIndex;
        }

        /**
         * Returns true if the marker was created with useMarkerWithLabel and the label is visible.
         * Note that this is not a part of the marker interface.
         */
        getLabelVisible() : boolean
        {
            return this.labelTooltip && this.labelTooltip.isOpen();
        }

        /**
         * Sets the visibility of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelVisible(visible: boolean)
        {
            if(this.labelTooltip) {
                if(visible !== this.getLabelVisible()) {
                    if(visible) {
                        this.map.openTooltip(this.labelTooltip);
                    } else {
                        this.map.closeTooltip(this.labelTooltip);
                    }
                }
            }
        }

        /**
         * Sets the label content. Only works on markers that have been created with useMarkerWithLabel.
         */
        getLabelContent(): string
        {
            return this.labelTooltip ? <string>this.labelTooltip.getContent() : '';
        }

        /**
         * Sets the content of a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelContent(content: string)
        {
            if(this.labelTooltip) {
                this.labelTooltip.setContent(content);
            }
        }

        /**
         * Gets the label anchor. Only works on markers that have been created with useMarkerWithLabel.
         */
        getLabelAnchor()
        {
            return null;
        }

        /**
         * Sets the anchor for a marker's label. Only works on markers that have been created with useMarkerWithLabel.
         * Note that this is not a part of the marker interface.
         */
        setLabelAnchor(anchor: IPoint)
        {
            ;
        }
    }

    /**
     * An object that wraps a map's native polyline object.
     */
    class MapPolyline implements IMapPolyline
    {
        id:         string | number;
        map:        L.Map;
        polyline:   L.Polyline;
        tag:        any;
        visible:    boolean;

        constructor(id: string | number, map: L.Map, nativePolyline: L.Polyline, tag: any, options: IMapPolylineSettings)
        {
            this.id = id;
            this.map = map;
            this.polyline = nativePolyline;
            this.tag = tag;
            this.visible = options.visible;

            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
        }

        destroy()
        {
            this.setVisible(false);
            this.polyline = null;
            this.map = null;
            this.tag = null;
        }

        getDraggable() : boolean
        {
            return false;
        }

        setDraggable(draggable: boolean)
        {
            ;
        }

        getEditable() : boolean
        {
            return false;
        }

        setEditable(editable: boolean)
        {
            ;
        }

        getVisible() : boolean
        {
            return this.visible;
        }

        setVisible(visible: boolean)
        {
            if(this.visible !== visible) {
                if(visible) {
                    this.polyline.addTo(this.map);
                } else {
                    this.polyline.removeFrom(this.map);
                }
                this.visible = visible;
            }
        }

        private _StrokeColour: string;
        getStrokeColour() : string
        {
            return this._StrokeColour;
        }
        setStrokeColour(value: string)
        {
            if(value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polyline.setStyle({
                    color: value
                });
            }
        }

        private _StrokeOpacity: number;
        getStrokeOpacity() : number
        {
            return this._StrokeOpacity;
        }
        setStrokeOpacity(value: number)
        {
            if(value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polyline.setStyle({
                    opacity: value
                });
            }
        }

        private _StrokeWeight: number;
        getStrokeWeight() : number
        {
            return this._StrokeWeight;
        }
        setStrokeWeight(value: number)
        {
            if(value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polyline.setStyle({
                    weight: value
                });
            }
        }

        getPath() : ILatLng[]
        {
            return VRS.leafletUtilities.fromLeafletLatLngArray(<L.LatLng[]>(this.polyline.getLatLngs()));
        }

        setPath(path: ILatLng[])
        {
            this.polyline.setLatLngs(VRS.leafletUtilities.toLeafletLatLngArray(path, this.map));
        }

        getFirstLatLng() : ILatLng
        {
            var result = null;
            var nativePath = <L.LatLng[]>this.polyline.getLatLngs();
            if(nativePath.length) result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[0]);

            return result;
        }

        getLastLatLng() : ILatLng
        {
            var result = null;
            var nativePath = <L.LatLng[]>this.polyline.getLatLngs();
            if(nativePath.length) result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[nativePath.length - 1]);

            return result;
        }
    }

    /**
     * An object that wraps a map's native circle object.
     * @constructor
     */
    class MapCircle implements IMapCircle
    {
        id:         string | number;
        circle:     L.Circle;
        map:        L.Map;
        tag:        any;
        visible:    boolean;

        /**
         * Creates a new object.
         * @param {string|number}           id                  The unique identifier of the circle object.
         * @param {L.Map}                   map                 The map to add the circle to or remove the circle from.
         * @param {L.Circle}                nativeCircle        The native object that is being wrapped.
         * @param {*}                       tag                 An object attached to the circle.
         * @param {IMapCircleSettings}      options             The options used when the circle was created.
        */
        constructor(id: string | number, map: L.Map, nativeCircle: L.Circle, tag: any, options: IMapCircleSettings)
        {
            this.id = id;
            this.circle = nativeCircle;
            this.map = map;
            this.tag = tag;
            this.visible = options.visible;

            this._FillOpacity = options.fillOpacity;
            this._FillColour = options.fillColor;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeColour = options.strokeColor;
            this._StrokeWeight = options.strokeWeight;
            this._ZIndex = options.zIndex;
        }

        destroy()
        {
            this.setVisible(false);
            this.circle = null;
            this.map = null;
            this.tag = null;
        }

        getBounds() : IBounds
        {
            return VRS.leafletUtilities.fromLeafletLatLngBounds(this.circle.getBounds());
        }

        getCenter() : ILatLng
        {
            return VRS.leafletUtilities.fromLeafletLatLng(this.circle.getLatLng());
        }
        setCenter(value: ILatLng)
        {
            this.circle.setLatLng(VRS.leafletUtilities.toLeafletLatLng(value, this.map));
        }

        getDraggable() : boolean
        {
            return false;
        }
        setDraggable(value: boolean)
        {
            ;
        }

        getEditable() : boolean
        {
            return false;
        }
        setEditable(value: boolean)
        {
            ;
        }

        getRadius() : number
        {
            return this.circle.getRadius();
        }
        setRadius(value: number)
        {
            this.circle.setRadius(value);
        }

        getVisible() : boolean
        {
            return this.visible;
        }
        setVisible(visible: boolean)
        {
            if(this.visible !== visible) {
                if(visible) {
                    this.circle.addTo(this.map);
                } else {
                    this.circle.removeFrom(this.map);
                }
                this.visible = visible;
            }
        }

        private _FillColour: string;
        getFillColor() : string
        {
            return this._FillColour;
        }
        setFillColor(value: string)
        {
            if(this._FillColour !== value) {
                this._FillColour = value;
                this.circle.setStyle({
                    fillColor: value
                });
            }
        }

        private _FillOpacity: number;
        getFillOpacity() : number
        {
            return this._FillOpacity;
        }
        setFillOpacity(value: number)
        {
            if(this._FillOpacity !== value) {
                this._FillOpacity = value;
                this.circle.setStyle({
                    fillOpacity: value
                });
            }
        }

        private _StrokeColour: string;
        getStrokeColor() : string
        {
            return this._StrokeColour;
        }
        setStrokeColor(value: string)
        {
            if(this._StrokeColour !== value) {
                this._StrokeColour = value;
                this.circle.setStyle({
                    color: value
                });
            }
        }

        private _StrokeOpacity: number;
        getStrokeOpacity() : number
        {
            return this._StrokeOpacity;
        }
        setStrokeOpacity(value: number)
        {
            if(this._StrokeOpacity !== value) {
                this._StrokeOpacity = value;
                this.circle.setStyle({
                    opacity: value
                });
            }
        }

        private _StrokeWeight: number;
        getStrokeWeight() : number
        {
            return this._StrokeWeight;
        }
        setStrokeWeight(value: number)
        {
            if(this._StrokeWeight !== value) {
                this._StrokeWeight = value;
                this.circle.setStyle({
                    weight: value
                });
            }
        }

        private _ZIndex: number;
        getZIndex() : number
        {
            return this._ZIndex;
        }
        setZIndex(value: number)
        {
            this._ZIndex = value;
        }
    }

    class MapControl extends L.Control
    {
        constructor(readonly element: JQuery | HTMLElement, options: L.ControlOptions)
        {
            super(options);
        }

        onAdd(map: L.Map): HTMLElement
        {
            var result = $('<div class="leaflet-control"></div>');
            result.append(this.element);

            return result[0];
        }
    }

    /**
     * Describes a polygon on a map.
     */
    class MapPolygon implements IMapPolygon
    {
        id:         string | number;
        map:        L.Map;
        polygon:    L.Polygon;
        tag:        any;
        visible:    boolean;

        constructor(id: string | number, nativeMap: L.Map, nativePolygon: L.Polygon, tag: any, options: IMapPolygonSettings)
        {
            this.id = id;
            this.map = nativeMap;
            this.polygon = nativePolygon;
            this.tag = tag;
            this.visible = options.visible;

            this._FillColour = options.fillColour;
            this._FillOpacity = options.fillOpacity;
            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
            this._ZIndex = options.zIndex;
        }

        destroy()
        {
            this.setVisible(false);
            this.map = null;
            this.polygon = null;
            this.tag = null;
        }

        getDraggable() : boolean
        {
            return false;
        }

        setDraggable(draggable: boolean)
        {
            ;
        }

        getEditable() : boolean
        {
            return false;
        }

        setEditable(editable: boolean)
        {
            ;
        }

        getVisible() : boolean
        {
            return this.visible;
        }

        setVisible(visible: boolean)
        {
            if(visible != this.visible) {
                if(visible) {
                    this.polygon.addTo(this.map);
                } else {
                    this.polygon.removeFrom(this.map);
                }
                this.visible = visible;
            }
        }

        getFirstPath() : ILatLng[]
        {
            return VRS.leafletUtilities.fromLeafletLatLngArray(<L.LatLng[]>this.polygon.getLatLngs());
        }

        setFirstPath(path: ILatLng[])
        {
            this.polygon.setLatLngs(path);
        }

        getPaths() : ILatLng[][]
        {
            // For now I'm just supporting single path polygons
            return <ILatLng[][]> [
                this.getFirstPath()
            ];
        }

        setPaths(paths: ILatLng[][])
        {
            // For now I'm just supporting single path polygons
            this.setFirstPath(paths[0]);
        }

        getClickable() : boolean
        {
            return this.polygon.options.interactive;
        }
        setClickable(value: boolean)
        {
            if(value !== this.getClickable()) {
                this.polygon.options.interactive = value;
            }
        }

        private _FillColour: string;
        getFillColour() : string
        {
            return this._FillColour;
        }
        setFillColour(value: string)
        {
            if(value !== this._FillColour) {
                this._FillColour = value;
                this.polygon.setStyle({ fillColor: value });
            }
        }

        private _FillOpacity: number;
        getFillOpacity() : number
        {
            return this._FillOpacity;
        }
        setFillOpacity(value: number)
        {
            if(value !== this._FillOpacity) {
                this._FillOpacity = value;
                this.polygon.setStyle({ fillOpacity: value });
            }
        }

        private _StrokeColour: string;
        getStrokeColour() : string
        {
            return this._StrokeColour;
        }
        setStrokeColour(value: string)
        {
            if(value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polygon.setStyle({ color: value });
            }
        }

        private _StrokeOpacity: number;
        getStrokeOpacity() : number
        {
            return this._StrokeOpacity;
        }
        setStrokeOpacity(value: number)
        {
            if(value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polygon.setStyle({ opacity: value });
            }
        }

        private _StrokeWeight: number;
        getStrokeWeight() : number
        {
            return this._StrokeWeight;
        }
        setStrokeWeight(value: number)
        {
            if(value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polygon.setStyle({ weight: value });
            }
        }

        private _ZIndex: number;
        getZIndex() : number
        {
            return this._ZIndex;
        }
        setZIndex(value: number)
        {
            if(value !== this._ZIndex) {
                this._ZIndex = value;
            }
        }
    }

    /**
     * A wrapper around a map's native info window.
     */
    class MapInfoWindow implements IMapInfoWindow
    {
        id:             string | number;
        map:            L.Map;
        infoWindow:     L.Popup;
        tag:            any;
        isOpen:         boolean;
        boundMarker:    MapMarker;

        /**
         * Creates a new object.
         * @param {string|number}           id                  The unique identifier of the info window
         * @param {L.Popup}                 nativeInfoWindow    The map's native info window object that this wraps.
         * @param {*}                       tag                 An abstract object that is associated with the info window.
         * @param {IMapInfoWindowSettings}  options             The options used to create the info window.
        */
        constructor(id: string | number, nativeMap: L.Map, nativeInfoWindow: L.Popup, tag: any, options: IMapInfoWindowSettings)
        {
            this.id = id;
            this.map = nativeMap;
            this.infoWindow = nativeInfoWindow;
            this.tag = tag;
            this.isOpen = false;

            this._DisableAutoPan = options.disableAutoPan;
            this._MaxWidth = options.maxWidth;
            this._PixelOffset = options.pixelOffset;
        }

        destroy()
        {
            this.infoWindow.setContent('');
            this.map = null;
            this.tag = null;
            this.infoWindow = null;
        }

        getContent() : Element
        {
            return <Element>this.infoWindow.getContent();
        }
        setContent(value: Element)
        {
            this.infoWindow.setContent(<any>value);
        }

        private _DisableAutoPan: boolean;
        getDisableAutoPan() : boolean
        {
            return this._DisableAutoPan;
        }
        setDisableAutoPan(value: boolean)
        {
            if(this._DisableAutoPan !== value) {
                this._DisableAutoPan = value;
                this.infoWindow.options.autoPan = !value;
            }
        }

        private _MaxWidth: number;
        getMaxWidth() : number
        {
            return this._MaxWidth;
        }
        setMaxWidth(value: number)
        {
            if(this._MaxWidth !== value) {
                this._MaxWidth = value;
                this.infoWindow.options.maxWidth = value;
            }
        }

        private _PixelOffset: ISize;
        getPixelOffset() : ISize
        {
            return this._PixelOffset;
        }
        setPixelOffset(value: ISize)
        {
            if(this._PixelOffset !== value) {
                this._PixelOffset = value;
                this.infoWindow.options.offset = VRS.leafletUtilities.toLeafletSize(value);
            }
        }

        getPosition() : ILatLng
        {
            return VRS.leafletUtilities.fromLeafletLatLng(this.infoWindow.getLatLng());
        }
        setPosition(value: ILatLng)
        {
            this.infoWindow.setLatLng(VRS.leafletUtilities.toLeafletLatLng(value, this.map));
        }

        getZIndex() : number
        {
            return 1;
        }
        setZIndex(value: number)
        {
            ;
        }

        open(mapMarker: MapMarker)
        {
            this.close();

            if(!this.isOpen) {
                this.isOpen = true;
                if(!mapMarker) {
                    this.map.openPopup(this.infoWindow);
                } else {
                    this.boundMarker = mapMarker;
                    var markerHeight = mapMarker.getIcon().size.height;
                    this.setPixelOffset({ width: 0, height: -markerHeight });
                    mapMarker.marker.bindPopup(this.infoWindow).openPopup();
                }
            }
        }

        close()
        {
            if(this.isOpen) {
                this.isOpen = false;
                if(!this.boundMarker) {
                    this.map.closePopup(this.infoWindow);
                } else {
                    if(this.boundMarker.marker) {
                        this.boundMarker.marker.closePopup();
                        this.boundMarker.marker.unbindPopup();
                    }
                    this.boundMarker = null;
                }
            }
        }
    }

    /**
     * A wrapper around the leaflet marker cluster plugin
     */
    class MapMarkerClusterer implements IMapMarkerClusterer
    {
        map:            L.Map;
        clusterGroup:   L.MarkerClusterGroup;
        maxZoom:        number;

        constructor(nativeMap: L.Map, settings: IMapMarkerClustererSettings)
        {
            this.map = nativeMap;
            this.maxZoom = settings.maxZoom;

            this.createClusterGroup();
        }

        private createClusterGroup()
        {
            var mapMarkers : L.Layer[] = null;
            if(this.clusterGroup) {
                mapMarkers = this.clusterGroup.getLayers();
                this.destroyClusterGroup();
            }

            var options : L.MarkerClusterGroupOptions = {
                disableClusteringAtZoom:    this.maxZoom + 1,
                spiderfyOnMaxZoom:          false,
                showCoverageOnHover:        true
            };
            this.clusterGroup = L.markerClusterGroup(options);

            if(mapMarkers && mapMarkers.length > 0) {
                this.clusterGroup.addLayers(mapMarkers);
            }

            this.map.addLayer(this.clusterGroup);
        }

        private destroyClusterGroup()
        {
            if(this.clusterGroup) {
                this.clusterGroup.removeLayers(this.clusterGroup.getLayers());
                this.map.removeLayer(this.clusterGroup);
                this.clusterGroup = null;
            }
        }

        getNative() : any
        {
            return this.clusterGroup;
        }

        getNativeType()
        {
            return 'OpenStreetMap';
        }

        getMaxZoom() : number
        {
            return this.maxZoom;
        }

        setMaxZoom(maxZoom: number)
        {
            if(maxZoom !== this.maxZoom) {
                this.maxZoom = maxZoom;
                this.createClusterGroup();
            }
        }

        addMarker(marker: IMapMarker, noRepaint?: boolean)
        {
            if(marker) {
                var nativeMarker = VRS.leafletUtilities.toLeafletMapMarker(marker);
                nativeMarker.remove();
                this.clusterGroup.addLayer(nativeMarker);
            }
        }

        addMarkers(markers: IMapMarker[], noRepaint?: boolean)
        {
            if(markers) {
                var nativeMarkers = VRS.leafletUtilities.toLeafletMapMarkers(markers);
                var len = nativeMarkers.length;
                for(var i = 0;i < len;++i) {
                    nativeMarkers[i].remove();
                }
                this.clusterGroup.addLayers(nativeMarkers);
            }
        }

        removeMarker(marker: IMapMarker, noRepaint?: boolean)
        {
            if(marker) {
                this.clusterGroup.removeLayer(VRS.leafletUtilities.toLeafletMapMarker(marker));
            }
        }

        removeMarkers(markers: IMapMarker[], noRepaint?: boolean)
        {
            if(markers) {
                this.clusterGroup.removeLayers(VRS.leafletUtilities.toLeafletMapMarkers(markers));
            }
        }

        repaint()
        {
            this.clusterGroup.refreshClusters();
        }
    }

    /**
     * The state held for a layer.
     */
    class MapLayer
    {
        map: L.Map;
        layer: L.TileLayer;

        constructor(nativeMap: L.Map, nativeLayer: L.TileLayer)
        {
            this.map = nativeMap;
            this.layer = nativeLayer;
        }

        destroy()
        {
            this.map = null;
            this.layer = null;
        }

        getOpacity() : number
        {
            return this.layer.options.opacity;
        }

        setOpacity(value: number)
        {
            this.layer.setOpacity(value);
        }
    }

    /**
     * The state held for every map plugin object.
     */
    class MapPluginState
    {
        /**
         * The map's container.
         */
        mapContainer: JQuery = undefined;

        /**
         * The leaflet map.
         */
        map: L.Map = undefined;

        /**
         * The name of the current map's tile server settings.
         */
        mapName: string = undefined;

        /**
         * The map tile layer.
         */
        tileLayer: L.TileLayer = undefined;

        /**
         * An associative array of marker IDs to markers.
         */
        markers: { [markerId: string]: MapMarker } = {};

        /**
         * An associative array of polyline IDs to polylines.
         */
        polylines: { [polylineId: string]: MapPolyline } = {};

        /**
         * An associative array of circle IDs to circles.
         */
        circles: { [circleId: string]: MapCircle } = {};

        /**
         * An associative array of polygon IDs to polygons.
         */
        polygons: { [polygonId: string]: MapPolygon } = {};

        /**
         * An associative array of info window IDs to info windows.
         */
        infoWindows: { [infoWindowId: string]: MapInfoWindow } = {};

        /**
         * An associative array of layer names to open layer objects.
         */
        layers: { [layerName: string]: MapLayer } = {};

        /**
         * True if the map's events have been hooked.
         */
        eventsHooked = false;

        /**
         * The map centre that we're setting. Used to prevent recursive map events
         * while moving the map.
         */
        settingCenter: ILatLng = undefined;

        /**
         * The default brightness specified by the tile server settings.
         */
        defaultBrightness: number = undefined;

        /**
         * The map brightness where 100 = normal brightness and 150 is 50% extra bright.
         */
        brightness = 100;
    }

    /**
     * A jQuery plugin that wraps Leaflet maps.
     */
    class MapPlugin extends JQueryUICustomWidget implements IMap
    {
        options: IMapOptions;
        private _EventPluginName = 'vrsMap';

        constructor()
        {
            super();
            this.options = VRS.jQueryUIHelper.getMapOptions();
        }

        private _getState() : MapPluginState
        {
            var result = this.element.data('mapPluginState');
            if(result === undefined) {
                result = new MapPluginState();
                this.element.data('mapPluginState', result);
            }

            return result;
        }

        _create()
        {
            if(this.options.useServerDefaults && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    this.options.center = { lat: config.InitialLatitude, lng: config.InitialLongitude };
                    this.options.mapTypeId = config.InitialMapType;
                    this.options.zoom = config.InitialZoom;
                }
            }

            var state = this._getState();
            state.mapContainer = $('<div />')
                .addClass('vrsMap')
                .appendTo(this.element);

            if(VRS.refreshManager) {
                VRS.refreshManager.registerTarget(this.element, this._targetResized, this);
            }

            if(this.options.afterCreate) {
                this.options.afterCreate(this);
            }
            if(this.options.openOnCreate) {
                this.open();
            }
        }

        _destroy()
        {
            var state = this._getState();

            this._hookEvents(state, false);

            if(VRS.refreshManager) VRS.refreshManager.unregisterTarget(this.element);
            if(state.mapContainer) state.mapContainer.remove();
        }

        _hookEvents(state: MapPluginState, hook: boolean)
        {
            if(state.map) {
                if((hook && !state.eventsHooked) || (!hook && state.eventsHooked)) {
                    state.eventsHooked = hook;

                    if(hook)    state.map.on ('resize', this._map_resized, this);
                    else        state.map.off('resize', this._map_resized, this);

                    if(hook)    state.map.on ('move', this._map_moved, this);
                    else        state.map.off('move', this._map_moved, this);

                    if(hook)    state.map.on ('moveend', this._map_moveEnded, this);
                    else        state.map.off('moveend', this._map_moveEnded, this);

                    if(hook)    state.map.on ('click', this._map_clicked, this);
                    else        state.map.off('click', this._map_clicked, this);

                    if(hook)    state.map.on ('dblclick', this._map_doubleClicked, this);
                    else        state.map.off('dblclick', this._map_doubleClicked, this);

                    if(hook)    state.map.on ('zoomend', this._map_zoomEnded, this);
                    else        state.map.off('zoomend', this._map_zoomEnded, this);

                    if(hook)    state.tileLayer.on ('load', this._tileLayer_loaded, this);
                    else        state.tileLayer.off('load', this._tileLayer_loaded, this);
                }
            }
        }

        _map_resized(e: L.ResizeEvent)
        {
            this._raiseBoundsChanged();
        }

        _map_moved(e: Event)
        {
            this._raiseCenterChanged();
        }

        _map_moveEnded(e: Event)
        {
            this._onIdle();
        }

        _map_clicked(e: MouseEvent)
        {
            this._userNotIdle();
            this._raiseClicked(e);
        }

        _map_doubleClicked(e: MouseEvent)
        {
            this._userNotIdle();
            this._raiseDoubleClicked(e);
        }

        _map_zoomEnded(e: Event)
        {
            this._raiseZoomChanged();
            this._onIdle();
        }

        _tileLayer_loaded(e:Event)
        {
            this._raiseTilesLoaded();
        }

        getNative(): any
        {
            return this._getState().map;
        }

        getNativeType() : string
        {
            return 'OpenStreetMap';
        }

        isOpen() : boolean
        {
            return !!this._getState().map;
        }

        isReady() : boolean
        {
            var state = this._getState();
            return !!state.map;
        }

        getBounds() : IBounds
        {
            return this._getBounds(this._getState());
        }
        private _getBounds(state: MapPluginState)
        {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLngBounds(state.map.getBounds()) : { tlLat: 0, tlLng: 0, brLat: 0, brLng: 0};
        }

        getCenter() : ILatLng
        {
            return this._getCenter(this._getState());
        }
        private _getCenter(state: MapPluginState)
        {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLng(state.map.getCenter()) : this.options.center;
        }

        setCenter(latLng: ILatLng)
        {
            this._setCenter(this._getState(), latLng);
        }
        private _setCenter(state: MapPluginState, latLng: ILatLng)
        {
            if(state.settingCenter === undefined || state.settingCenter === null || state.settingCenter.lat != latLng.lat || state.settingCenter.lng != latLng.lng) {
                try {
                    state.settingCenter = latLng;

                    if(state.map) state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(latLng, state.map));
                    else          this.options.center = latLng;
                } finally {
                    state.settingCenter = undefined;
                }
            }
        }

        getDraggable() : boolean
        {
            return this.options.draggable;
        }

        getMapType() : MapTypeEnum
        {
            return this._getMapType(this._getState());
        }
        private _getMapType(state: MapPluginState) : MapTypeEnum
        {
            return this.options.mapTypeId;
        }

        setMapType(mapType: MapTypeEnum)
        {
            this._setMapType(this._getState(), mapType);
        }
        private _setMapType(state: MapPluginState, mapType: MapTypeEnum)
        {
            this.options.mapTypeId = mapType;
        }

        getScrollWheel() : boolean
        {
            return this.options.scrollwheel;
        }

        getStreetView() : boolean
        {
            return this.options.streetViewControl;
        }

        getZoom() : number
        {
            return this._getZoom(this._getState());
        }
        private _getZoom(state: MapPluginState) : number
        {
            return state.map ? state.map.getZoom() : this.options.zoom;
        }

        setZoom(zoom: number)
        {
            this._setZoom(this._getState(), zoom);
        }
        private _setZoom(state: MapPluginState, zoom: number)
        {
            if(state.map) state.map.setZoom(zoom);
            else          this.options.zoom = zoom;
        }

        unhook(hookResult: IEventHandleJQueryUI)
        {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        }

        hookBoundsChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'boundsChanged', callback, forceThis);
        }
        private _raiseBoundsChanged()
        {
            this._trigger('boundsChanged');
        }

        hookBrightnessChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'brightnessChanged', callback, forceThis);
        }
        private _raiseBrightnessChanged()
        {
            this._trigger('brightnessChanged');
        }

        hookCenterChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'centerChanged', callback, forceThis);
        }
        private _raiseCenterChanged()
        {
            this._trigger('centerChanged');
        }

        hookClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'clicked', callback, forceThis);
        }
        private _raiseClicked(mouseEvent: Event)
        {
            this._trigger('clicked', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        hookDoubleClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'doubleClicked', callback, forceThis);
        }
        private _raiseDoubleClicked(mouseEvent: Event)
        {
            this._trigger('doubleClicked', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        hookIdle(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'idle', callback, forceThis);
        }
        private _raiseIdle()
        {
            this._trigger('idle');
        }

        hookMapTypeChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mapTypeChanged', callback, forceThis);
        }
        private _raiseMapTypeChanged()
        {
            this._trigger('mapTypeChanged');
        }

        hookRightClicked(callback: (event?: Event, data?: IMapMouseEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mouseEvent', callback, forceThis);
        }
        private _raiseRightClicked(mouseEvent: Event)
        {
            this._trigger('mouseEvent', null, <IMapMouseEventArgs>{ mouseEvent: mouseEvent });
        }

        hookTilesLoaded(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'tilesLoaded', callback, forceThis);
        }
        private _raiseTilesLoaded()
        {
            this._trigger('tilesLoaded');
        }

        hookZoomChanged(callback: (event?: Event) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'zoomChanged', callback, forceThis);
        }
        private _raiseZoomChanged()
        {
            this._trigger('zoomChanged');
        }

        hookMarkerClicked(callback: (event?: Event, data?: IMapMarkerEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerClicked', callback, forceThis);
        }
        raiseMarkerClicked(id: string | number)
        {
            this._trigger('markerClicked', null, <IMapMarkerEventArgs>{ id: id });
        }

        hookMarkerDragged(callback: (event?: Event, data?: IMapMarkerEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerDragged', callback, forceThis);
        }
        raiseMarkerDragged(id: string | number)
        {
            this._trigger('markerDragged', null, <IMapMarkerEventArgs>{ id: id });
        }

        hookInfoWindowClosedByUser(callback: (event?: Event, data?: IMapInfoWindowEventArgs) => void, forceThis?: Object) : IEventHandleJQueryUI
        {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'infoWindowClosedByUser', callback, forceThis);
        }
        private _raiseInfoWindowClosedByUser(id: string | number)
        {
            this._trigger('infoWindowClosedByUser', null, <IMapInfoWindowEventArgs>{ id: id });
        }

        /**
         * Called when the map becomes idle.
         */
        private _onIdle()
        {
            if(this.options.autoSaveState) this.saveState();
            this._raiseIdle();
        }

        /**
         * Called after the map type has changed.
         */
        private _onMapTypeChanged()
        {
            if(this.options.autoSaveState) this.saveState();
            this._raiseMapTypeChanged();
        }

        open(userOptions?: IMapOpenOptions)
        {
            var tileServerSettings = VRS.serverConfig.get().TileServerSettings;
            if(tileServerSettings) {
                var mapOptions: IMapOptions = $.extend(<IMapOptions>{}, userOptions, {
                    zoom:               this.options.zoom,
                    center:             this.options.center,
                    mapTypeControl:     this.options.showMapTypeControl,
                    mapTypeId:          this.options.mapTypeId,
                    streetViewControl:  this.options.streetViewControl,
                    scrollwheel:        this.options.scrollwheel,
                    scaleControl:       this.options.scaleControl,
                    draggable:          this.options.draggable,
                    showHighContrast:   this.options.showHighContrast,
                    controlStyle:       this.options.controlStyle,
                    controlPosition:    this.options.controlPosition,
                    mapControls:        this.options.mapControls
                });

                var overrideBrightness = tileServerSettings.DefaultBrightness;
                if(this.options.useStateOnOpen) {
                    var settings = this.loadState();
                    mapOptions.zoom = settings.zoom;
                    mapOptions.center = settings.center;
                    mapOptions.mapTypeId = settings.mapTypeId;

                    if(tileServerSettings.Name === settings.brightnessMapName && settings.brightness) {
                        overrideBrightness = settings.brightness;
                    }
                }

                mapOptions.zoom = this._normaliseZoom(mapOptions.zoom, tileServerSettings.MinZoom, tileServerSettings.MaxZoom);

                var leafletOptions: L.MapOptions = {
                    attributionControl:     true,
                    zoom:                   mapOptions.zoom,
                    center:                 VRS.leafletUtilities.toLeafletLatLng(mapOptions.center, null),
                    scrollWheelZoom:        mapOptions.scrollwheel,
                    dragging:               mapOptions.draggable,
                    zoomControl:            mapOptions.scaleControl
                };

                var state = this._getState();
                state.map = L.map(state.mapContainer[0], leafletOptions);
                state.mapName = tileServerSettings.Name;
                state.defaultBrightness = tileServerSettings.DefaultBrightness;

                var tileServerOptions = this._buildTileServerOptions(state, tileServerSettings, overrideBrightness);
                state.tileLayer = L.tileLayer(tileServerSettings.Url, tileServerOptions);
                state.tileLayer.addTo(state.map);

                this._hookEvents(state, true);

                if(mapOptions.mapControls) {
                    $.each(mapOptions.mapControls, (idx, ctrl) => {
                        this.addControl(ctrl.control, ctrl.position);
                    });
                }

                var waitUntilReady = () => {
                    if(this.options.waitUntilReady && !this.isReady()) {
                        setTimeout(waitUntilReady, 100);
                    } else {
                        if(this.options.afterOpen) this.options.afterOpen(this);
                    }
                };
                waitUntilReady();
            }
        }

        private _buildTileServerOptions(state: MapPluginState, settings: ITileServerSettings, overrideBrightness: number) : L.TileLayerOptions
        {
            var result: L.TileLayerOptions = {
                attribution:    this._attributionToHtml(settings.Attribution),
                detectRetina:   settings.DetectRetina,
                zoomReverse:    settings.ZoomReverse,
            };

            if(VRS.globalOptions.mapLeafletNoWrap) {
                result.noWrap = true;
            }

            if(settings.ClassName) {
                result.className = settings.ClassName;
            }
            if(!settings.IsLayer) {
                var brightness = overrideBrightness && !isNaN(overrideBrightness) ? overrideBrightness : settings.DefaultBrightness;
                result.className = this._setBrightnessClass(result.className, state.brightness, brightness);
                state.brightness = brightness;
            } else {
                if(settings.DefaultOpacity > 0 && settings.DefaultOpacity < 100) {
                    result.opacity = settings.DefaultOpacity / 100;
                }
            }
            if(settings.MaxNativeZoom !== null && settings.MaxNativeZoom !== undefined) {
                result.maxNativeZoom = settings.MaxNativeZoom;
            }
            if(settings.MinNativeZoom !== null && settings.MinNativeZoom !== undefined) {
                result.minNativeZoom = settings.MinNativeZoom;
            }
            if(settings.MaxZoom !== null && settings.MaxZoom !== undefined) {
                result.maxZoom = settings.MaxZoom;
            }
            if(settings.MinZoom !== null && settings.MinZoom !== undefined) {
                result.minZoom = settings.MinZoom;
            }
            if(settings.Subdomains !== null && settings.Subdomains !== undefined) {
                result.subdomains = settings.Subdomains;
            }
            if(settings.ZoomOffset !== null && settings.ZoomOffset !== undefined) {
                result.zoomOffset = settings.ZoomOffset;
            }
            if(settings.IsTms) {
                result.tms = true;
            }
            if(settings.ErrorTileUrl != null) {
                result.errorTileUrl = settings.ErrorTileUrl;
            }

            var countExpandos = settings.ExpandoOptions === null || settings.ExpandoOptions === undefined ? 0 : settings.ExpandoOptions.length;
            for(var i = 0;i < countExpandos;++i) {
                var expando = settings.ExpandoOptions[i];
                result[expando.Option] = VRS.stringUtility.htmlEscape(expando.Value);
            }

            return result;
        }

        private _attributionToHtml(attribution: string) : string
        {
            var result = VRS.stringUtility.htmlEscape(attribution);

            result = result.replace(/\[c\]/g, '&copy;');
            result = result.replace(/\[\/a\]/g, '</a>');
            result = result.replace(/\[a href=(.+?)\]/g, '<a href="$1">');

            return result;
        }

        private _normaliseZoom(zoom: number, minZoom: number, maxZoom: number) : number
        {
            var result = zoom;

            if(result !== undefined && result !== null) {
                if(minZoom !== undefined && minZoom !== null && result < minZoom) {
                    result = minZoom;
                }
                if(maxZoom !== undefined && maxZoom !== null && result > maxZoom) {
                    result = maxZoom;
                }
            }

            return result;
        }

        private _userNotIdle()
        {
            if(VRS.timeoutManager) VRS.timeoutManager.resetTimer();
        }

        refreshMap()
        {
            var state = this._getState();
            if(state.map) {
                state.map.invalidateSize();
            }
        }

        panTo(mapCenter: ILatLng)
        {
            this._panTo(mapCenter, this._getState());
        }
        private _panTo(mapCenter: ILatLng, state: MapPluginState)
        {
            if(state.settingCenter === undefined || state.settingCenter === null || state.settingCenter.lat != mapCenter.lat || state.settingCenter.lng != mapCenter.lng) {
                try {
                    state.settingCenter = mapCenter;

                    if(state.map) state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(mapCenter, state.map));
                    else          this.options.center = mapCenter;
                } finally {
                    state.settingCenter = undefined;
                }
            }
        }

        fitBounds(bounds: IBounds)
        {
            var state = this._getState();
            if(state.map) {
                state.map.fitBounds(VRS.leafletUtilities.toLeaftletLatLngBounds(bounds, state.map));
            }
        }

        saveState()
        {
            var settings = this._createSettings();
            VRS.configStorage.save(this._persistenceKey(), settings);
        }

        loadState() : IMapSaveState
        {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            return $.extend(this._createSettings(), savedSettings);
        }

        applyState(config: IMapSaveState)
        {
            config = config || <IMapSaveState>{};
            var state = this._getState();

            if(config.center)                       this._setCenter(state, config.center);
            if(config.zoom || config.zoom === 0)    this._setZoom(state, config.zoom);
            if(config.mapTypeId)                    this._setMapType(state, config.mapTypeId);

            if(state.mapName === config.brightnessMapName) {
                this.setMapBrightness(config.brightness);
            }
        };

        loadAndApplyState()
        {
            this.applyState(this.loadState());
        }

        private _persistenceKey() : string
        {
            return 'vrsMapState-' + (this.options.name || 'default');
        }

        private _createSettings() : IMapSaveState
        {
            var state = this._getState();

            return {
                zoom:               this._getZoom(state),
                mapTypeId:          this._getMapType(state),
                center:             this._getCenter(state),
                brightnessMapName:  state.mapName,
                brightness:         state.brightness === state.defaultBrightness ? 0 : state.brightness
            };
        }

        addMarker(id: string | number, userOptions: IMapMarkerSettings): IMapMarker
        {
            var result: MapMarker;

            var state = this._getState();
            if(state.map) {
                if(userOptions.zIndex === null || userOptions.zIndex === undefined) {
                    userOptions.zIndex = 0;
                }
                if(userOptions.draggable) {
                    userOptions.clickable = true;
                }
                var leafletOptions: L.MarkerOptions = {
                    interactive:    userOptions.clickable !== undefined ? userOptions.clickable : true,
                    draggable:      userOptions.draggable !== undefined ? userOptions.draggable : false,
                    zIndexOffset:   userOptions.zIndex,
                };
                if(userOptions.icon) {
                    leafletOptions.icon = VRS.leafletUtilities.toLeafletIcon(userOptions.icon);
                }
                if(userOptions.tooltip) {
                    leafletOptions.title = userOptions.tooltip;
                }

                var position = userOptions.position ? VRS.leafletUtilities.toLeafletLatLng(userOptions.position, state.map) : state.map.getCenter();

                this.destroyMarker(id);
                var nativeMarker = L.marker(position, leafletOptions);
                if(userOptions.visible) {
                    nativeMarker.addTo(state.map);
                }
                result = new MapMarker(id, this, state.map, nativeMarker, leafletOptions, userOptions);
                state.markers[id] = result;
            }

            return result;
        }

        getMarker(idOrMarker: string | number | IMapMarker): IMapMarker
        {
            if(idOrMarker instanceof MapMarker) return idOrMarker;
            var state = this._getState();
            return state.markers[<string | number>idOrMarker];
        }

        destroyMarker(idOrMarker: string | number | IMapMarker)
        {
            var state = this._getState();
            var marker = <MapMarker>this.getMarker(idOrMarker);
            if(marker) {
                marker.destroy();
                delete state.markers[marker.id];
                marker.id = null;
            }
        }

        centerOnMarker(idOrMarker: string | number | IMapMarker)
        {
            var state = this._getState();
            var marker = <MapMarker>this.getMarker(idOrMarker);
            if(marker) {
                this.setCenter(marker.getPosition());
            }
        }

        createMapMarkerClusterer(settings?: IMapMarkerClustererSettings): IMapMarkerClusterer
        {
            var result: MapMarkerClusterer = null;

            var state = this._getState();
            if(state.map) {
                var options = $.extend(<IMapMarkerClustererSettings>{}, settings, {
                });

                // The clusterer as implemented does work but it has two problems:
                //  1. The info window flickers when markers are moved. I think it may be related to this issue here:
                //     https://github.com/Leaflet/Leaflet.markercluster/issues/774 [Using marker.setLatLng triggers paint flashing]
                //  2. It will not cluster single aircraft without permanently modifying their icon.
                //
                // Unfortunately (1) is a show-stopper, it looks awful. I don't think leaflet.markercluster is going to work for
                // VRS until the issue is resolved. I've left the code in place but commented out the create call, if you want to
                // put the clusterer back then just uncomment this line:

                // result = new MapMarkerClusterer(state.map, options);
            }

            return result;
        }

        addPolyline(id: string | number, userOptions: IMapPolylineSettings): IMapPolyline
        {
            var result: MapPolyline;

            var state = this._getState();
            if(state.map) {
                var options = $.extend(<IMapPolylineSettings>{}, userOptions, {
                    visible: true
                });
                var leafletOptions: L.PolylineOptions = {
                    color:      options.strokeColour || '#000000'
                };
                if(options.strokeOpacity || leafletOptions.opacity === 0) leafletOptions.opacity = options.strokeOpacity;
                if(options.strokeWeight || leafletOptions.weight === 0) leafletOptions.weight = options.strokeWeight;

                var path: L.LatLng[] = [];
                if(options.path) path = VRS.leafletUtilities.toLeafletLatLngArray(options.path, state.map);

                this.destroyPolyline(id);
                var polyline = L.polyline(path, leafletOptions);
                if(options.visible) {
                    polyline.addTo(state.map);
                }

                result = new MapPolyline(id, state.map, polyline, options.tag, {
                    strokeColour:   options.strokeColour,
                    strokeOpacity:  options.strokeOpacity,
                    strokeWeight:   options.strokeWeight
                });
                state.polylines[id] = result;
            }

            return result;
        }

        getPolyline(idOrPolyline: string | number | IMapPolyline): IMapPolyline
        {
            if(idOrPolyline instanceof MapPolyline) return idOrPolyline;
            var state = this._getState();
            return state.polylines[<string | number>idOrPolyline];
        }

        destroyPolyline(idOrPolyline: string | number | IMapPolyline)
        {
            var state = this._getState();
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            if(polyline) {
                polyline.destroy();
                delete state.polylines[polyline.id];
                polyline.id = null;
            }
        }

        trimPolyline(idOrPolyline: string | number | IMapPolyline, countPoints: number, fromStart: boolean): IMapTrimPolylineResult
        {
            var emptied = false;
            var countRemoved = 0;

            if(countPoints > 0) {
                var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
                var points = <L.LatLng[]>polyline.polyline.getLatLngs();
                var length = points.length;
                if(length < countPoints) countPoints = length;
                if(countPoints > 0) {
                    countRemoved = countPoints;
                    emptied = countPoints === length;

                    if(emptied) {
                        points = [];
                    } else {
                        if(fromStart) {
                            points.splice(0, countPoints);
                        } else {
                            points.splice(length - countPoints, countPoints);
                        }
                    }
                    polyline.polyline.setLatLngs(points);
                }
            }

            return { emptied: emptied, countRemoved: countRemoved };
        }

        removePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number)
        {
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            var points = <L.LatLng[]>polyline.polyline.getLatLngs();
            points.splice(index, 1);
            polyline.polyline.setLatLngs(points);
        }

        appendToPolyline(idOrPolyline: string | number | IMapPolyline, path: ILatLng[], toStart: boolean)
        {
            var length = !path ? 0 : path.length;
            if(length > 0) {
                var state = this._getState();
                var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
                var points = <L.LatLng[]>polyline.polyline.getLatLngs();
                var insertAt = toStart ? 0 : -1;
                for(var i = 0;i < length;++i) {
                    var leafletPoint = VRS.leafletUtilities.toLeafletLatLng(path[i], state.map);
                    if(toStart) {
                        points.splice(insertAt++, 0, leafletPoint);
                    } else {
                        points.push(leafletPoint);
                    }
                }
                polyline.polyline.setLatLngs(points);
            }
        }

        replacePolylinePointAt(idOrPolyline: string | number | IMapPolyline, index: number, point: ILatLng)
        {
            var polyline = <MapPolyline>this.getPolyline(idOrPolyline);
            var points = <L.LatLng[]>polyline.polyline.getLatLngs();
            var length = points.length;
            if(index === -1) index = length - 1;
            if(index >= 0 && index < length) {
                var state = this._getState();
                points.splice(index, 1, VRS.leafletUtilities.toLeafletLatLng(point, state.map));
                polyline.polyline.setLatLngs(points);
            }
        }

        addPolygon(id: string | number, userOptions: IMapPolygonSettings): IMapPolygon
        {
            var result: MapPolygon;

            var state = this._getState();
            if(state.map) {
                var options: IMapPolygonSettings = $.extend(<IMapPolygonSettings>{}, userOptions, {
                    visible: true
                });
                var leafletOptions: L.PolylineOptions = {
                    color: options.strokeColour || '#000000',
                    fillColor: options.fillColour || '#ffffff',
                };
                if(options.strokeOpacity || leafletOptions.opacity === 0)   leafletOptions.opacity = options.strokeOpacity;
                if(options.fillOpacity || leafletOptions.fillOpacity === 0) leafletOptions.fillOpacity = options.fillOpacity;
                if(options.strokeWeight || leafletOptions.weight === 0)     leafletOptions.weight = options.strokeWeight;

                var paths: L.LatLng[] = [];
                if(options.paths) paths = VRS.leafletUtilities.toLeafletLatLngArray(options.paths[0], state.map);

                this.destroyPolygon(id);
                var polygon = new L.Polygon(paths, leafletOptions);
                if(options.visible) {
                    polygon.addTo(state.map);
                }
                result = new MapPolygon(id, state.map, polygon, userOptions.tag, userOptions);
                state.polygons[id] = result;
            }

            return result;
        }

        getPolygon(idOrPolygon: string | number | IMapPolygon): IMapPolygon
        {
            if(idOrPolygon instanceof MapPolygon) return idOrPolygon;
            var state = this._getState();
            return state.polygons[<string | number>idOrPolygon];
        }

        destroyPolygon(idOrPolygon: string | number | IMapPolygon)
        {
            var state = this._getState();
            var polygon = <MapPolygon>this.getPolygon(idOrPolygon);
            if(polygon) {
                polygon.destroy();
                delete state.polygons[polygon.id];
                polygon.id = null;
            }
        }

        addCircle(id: string | number, userOptions: IMapCircleSettings): IMapCircle
        {
            var result: MapCircle = null;

            var state = this._getState();
            if(state.map) {
                var options: IMapCircleSettings = $.extend({}, userOptions, {
                    visible: true
                });
                var leafletOptions: L.CircleMarkerOptions = {
                    fillColor:      options.fillColor || '#000',
                    fillOpacity:    options.fillOpacity !== null && options.fillOpacity !== undefined ? options.fillOpacity : 0,
                    color:          options.strokeColor || '#000',
                    opacity:        options.strokeOpacity !== null && options.strokeOpacity !== undefined ? options.strokeOpacity : 1,
                    weight:         options.strokeWeight !== null && options.strokeWeight !== undefined ? options.strokeWeight : 1,
                    radius:         options.radius || 0
                };
                var centre = VRS.leafletUtilities.toLeafletLatLng(options.center, state.map);

                this.destroyCircle(id);
                var circle = L.circle(centre, leafletOptions);
                if(options.visible) {
                    circle.addTo(state.map);
                }
                result = new MapCircle(id, state.map, circle, options.tag, options);
                state.circles[id] = result;
            }

            return result;
        }

        getCircle(idOrCircle: string | number | IMapCircle): IMapCircle
        {
            if(idOrCircle instanceof MapCircle) return idOrCircle;
            var state = this._getState();
            return state.circles[<string | number>idOrCircle];
        }

        destroyCircle(idOrCircle: string | number | IMapCircle)
        {
            var state = this._getState();
            var circle = <MapCircle>this.getCircle(idOrCircle);
            if(circle) {
                circle.destroy();
                delete state.circles[circle.id];
                circle.id = null;
            }
        }

        getUnusedInfoWindowId(): string
        {
            var result;

            var state = this._getState();
            for(var i = 1;i > 0;++i) {
                result = 'autoID' + i;
                if(!state.infoWindows[result]) break;
            }

            return result;
        }

        addInfoWindow(id: string | number, userOptions: IMapInfoWindowSettings): IMapInfoWindow
        {
            var result: MapInfoWindow = null;

            var state = this._getState();
            if(state.map) {
                var options: IMapInfoWindowSettings = $.extend({
                    visible: true
                }, userOptions);
                var leafletOptions: L.PopupOptions = {
                    autoPan:        !!!options.disableAutoPan,
                    autoClose:      false,
                    closeOnClick:   false,
                    maxWidth:       options.maxWidth
                };
                if(options.pixelOffset) {
                    leafletOptions.offset = VRS.leafletUtilities.toLeafletSize(options.pixelOffset);
                }

                this.destroyInfoWindow(id);
                var infoWindow = new L.Popup(leafletOptions);
                if(options.position) {
                    infoWindow.setLatLng(VRS.leafletUtilities.toLeafletLatLng(options.position, state.map));
                }
                if(options.content) {
                    infoWindow.setContent(<HTMLElement>options.content);
                }

                result = new MapInfoWindow(id, state.map, infoWindow, options.tag, options);
                state.infoWindows[id] = result;
            }

            return result;
        }

        getInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow): IMapInfoWindow
        {
            if(idOrInfoWindow instanceof MapInfoWindow) return idOrInfoWindow;
            var state = this._getState();
            return state.infoWindows[<string | number>idOrInfoWindow];
        }

        destroyInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow)
        {
            var state = this._getState();
            var infoWindow = <MapInfoWindow>this.getInfoWindow(idOrInfoWindow);
            if(infoWindow) {
                this.closeInfoWindow(infoWindow);
                infoWindow.destroy();
                delete state.infoWindows[infoWindow.id];
                infoWindow.id = null;
            }
        }

        openInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow, mapMarker?: IMapMarker)
        {
            var state = this._getState();
            var infoWindow = <MapInfoWindow>this.getInfoWindow(idOrInfoWindow);
            if(infoWindow && state.map) {
                infoWindow.open(<MapMarker>mapMarker);
            }
        }

        closeInfoWindow(idOrInfoWindow: string | number | IMapInfoWindow)
        {
            var infoWindow = <MapInfoWindow>this.getInfoWindow(idOrInfoWindow);
            infoWindow.close();
        }

        addControl(element: JQuery | HTMLElement, mapPosition: MapPositionEnum)
        {
            var state = this._getState();
            if(state.map) {
                var controlOptions: L.ControlOptions = {
                    position: VRS.leafletUtilities.toLeafletMapPosition(mapPosition)
                };
                var control = new MapControl(element, controlOptions);
                control.addTo(state.map);
            }
        }


        //
        // MAP LAYER METHODS
        //

        addLayer(layerTileSettings: ITileServerSettings, opacity: number)
        {
            var state = this._getState();
            if(state.map && layerTileSettings && layerTileSettings.IsLayer && layerTileSettings.Name) {
                var mapLayer = state.layers[layerTileSettings.Name];
                if(!mapLayer) {
                    var layerOptions = this._buildTileServerOptions(state, layerTileSettings, 100);

                    if(opacity !== null && opacity !== undefined) {
                        layerOptions.opacity = Math.min(1.0, Math.max(0, opacity / 100.0));
                    }
                    $.extend({
                        opacity: 1.0
                    }, layerOptions);

                    var tileLayer = L.tileLayer(layerTileSettings.Url, layerOptions);
                    tileLayer.addTo(state.map);

                    mapLayer = new MapLayer(state.map, tileLayer);
                    state.layers[layerTileSettings.Name] = mapLayer;
                }
            }
        }

        destroyLayer(layerName: string)
        {
            var state = this._getState();
            if(state.map && layerName) {
                var mapLayer = state.layers[layerName];
                if(mapLayer) {
                    mapLayer.layer.removeFrom(state.map);
                    mapLayer.destroy();
                    delete state.layers[layerName];
                }
            }
        }

        hasLayer(layerName: string) : boolean
        {
            var state = this._getState();
            return !!(state.map && layerName && state.layers[layerName]);
        }

        getLayerOpacity(layerName: string)
        {
            var result: number = undefined;

            var state = this._getState();
            if(state.map && layerName) {
                var mapLayer = state.layers[layerName];
                if(mapLayer) {
                    result = mapLayer.getOpacity() * 100.0;
                }
            }

            return result;
        }

        setLayerOpacity(layerName: string, opacity: number)
        {
            var state = this._getState();
            if(state.map && layerName && !isNaN(opacity)) {
                var mapLayer = state.layers[layerName];
                if(mapLayer) {
                    mapLayer.setOpacity(Math.min(1.0, Math.max(0.0, opacity / 100.0)));
                }
            }
        }


        //
        // BRIGHTNESS METHODS
        //
        getCanSetMapBrightness() : boolean
        {
            return true;
        }

        getDefaultMapBrightness() : number
        {
            var state = this._getState();
            return state.defaultBrightness;
        }

        getMapBrightness() : number
        {
            var state = this._getState();
            return state.brightness;
        }

        setMapBrightness(value: number)
        {
            var state = this._getState();
            if(value && !isNaN(value) && state.brightness !== value) {
                if(!state.map) {
                    state.brightness = value;
                } else {
                    var container = state.tileLayer.getContainer();
                    container.className = this._setBrightnessClass(
                        container.className,
                        state.brightness,
                        value
                    );
                    state.brightness = value;
                }

                this.saveState();
                this._raiseBrightnessChanged();
            }
        }

        private _setBrightnessClass(currentClassName: string, currentBrightness: number, newBrightness: number) : string
        {
            var result = currentClassName;

            if(currentBrightness !== newBrightness) {
                if(!result) {
                    result = '';
                }

                var currentBrightnessClass = this._classNameForMapBrightness(currentBrightness);
                if(currentBrightnessClass !== '') {
                    result = result
                        .replace(currentBrightnessClass, '')
                        .trim();
                }

                var newBrightnessClass = this._classNameForMapBrightness(newBrightness);
                if(newBrightnessClass !== '') {
                    if(result.length > 0) {
                        result += ' ';
                    }
                    result += newBrightnessClass;
                }
            }

            return result;
        }

        private _classNameForMapBrightness(brightness: number) : string
        {
            var result = '';

            if(brightness && !isNaN(brightness) && brightness > 0 && brightness <= 150 && brightness !== 100 && brightness / 10 === Math.floor(brightness / 10)) {
                result = 'vrs-brightness-' + brightness;
            }

            return result;
        }


        //
        // VRS EVENTS SUBSCRIBED
        //


        /**
         * Called when the refresh manager indicates that one of our parents has resized, or done something that we need
         * to refresh for.
         */
        private _targetResized()
        {
            var state = this._getState();

            var center = this._getCenter(state);
            this.refreshMap();
            this._setCenter(state, center);
        }
    }

    $.widget('vrs.vrsMap', new MapPlugin());
}

declare interface JQuery
{
    vrsMap();
    vrsMap(options: VRS.IMapOptions);
    vrsMap(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
