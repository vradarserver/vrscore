var __extends = (this && this.__extends) || (function () {
    var extendStatics = Object.setPrototypeOf ||
        ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
        function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var VRS;
(function (VRS) {
    VRS.globalOptions = VRS.globalOptions || {};
    VRS.globalOptions.mapGoogleMapHttpUrl = VRS.globalOptions.mapGoogleMapHttpUrl || '';
    VRS.globalOptions.mapGoogleMapHttpsUrl = VRS.globalOptions.mapGoogleMapHttpsUrl || '';
    VRS.globalOptions.mapGoogleMapTimeout = VRS.globalOptions.mapGoogleMapTimeout || 30000;
    VRS.globalOptions.mapGoogleMapUseHttps = VRS.globalOptions.mapGoogleMapUseHttps !== undefined ? VRS.globalOptions.mapGoogleMapUseHttps : true;
    VRS.globalOptions.mapShowStreetView = VRS.globalOptions.mapShowStreetView !== undefined ? VRS.globalOptions.mapShowStreetView : false;
    VRS.globalOptions.mapShowHighContrastStyle = VRS.globalOptions.mapShowHighContrastStyle !== undefined ? VRS.globalOptions.mapShowHighContrastStyle : true;
    VRS.globalOptions.mapHighContrastMapStyle = VRS.globalOptions.mapHighContrastMapStyle !== undefined ? VRS.globalOptions.mapHighContrastMapStyle : [];
    VRS.globalOptions.mapScrollWheelActive = VRS.globalOptions.mapScrollWheelActive !== undefined ? VRS.globalOptions.mapScrollWheelActive : true;
    VRS.globalOptions.mapDraggable = VRS.globalOptions.mapDraggable !== undefined ? VRS.globalOptions.mapDraggable : true;
    VRS.globalOptions.mapShowPointsOfInterest = VRS.globalOptions.mapShowPointsOfInterest !== undefined ? VRS.globalOptions.mapShowPointsOfInterest : false;
    VRS.globalOptions.mapShowScaleControl = VRS.globalOptions.mapShowScaleControl !== undefined ? VRS.globalOptions.mapShowScaleControl : true;
    VRS.globalOptions.mapLeafletNoWrap = VRS.globalOptions.mapLeafletNoWrap !== undefined ? VRS.globalOptions.mapLeafletNoWrap : true;
    var LeafletUtilities = (function () {
        function LeafletUtilities() {
        }
        LeafletUtilities.prototype.fromLeafletLatLng = function (latLng) {
            return latLng;
        };
        LeafletUtilities.prototype.toLeafletLatLng = function (latLng, map) {
            if (latLng instanceof L.LatLng) {
                return latLng;
            }
            else if (latLng) {
                return map ? map.wrapLatLng([latLng.lat, latLng.lng]) : new L.LatLng(latLng.lat, latLng.lng);
            }
            return null;
        };
        LeafletUtilities.prototype.fromLeafletLatLngArray = function (latLngArray) {
            return latLngArray;
        };
        LeafletUtilities.prototype.toLeafletLatLngArray = function (latLngArray, map) {
            latLngArray = latLngArray || [];
            var result = [];
            var len = latLngArray.length;
            for (var i = 0; i < len; ++i) {
                result.push(this.toLeafletLatLng(latLngArray[i], map));
            }
            return result;
        };
        LeafletUtilities.prototype.fromLeafletLatLngBounds = function (bounds) {
            if (!bounds) {
                return null;
            }
            return {
                tlLat: bounds.getNorth(),
                tlLng: bounds.getWest(),
                brLat: bounds.getSouth(),
                brLng: bounds.getEast()
            };
        };
        LeafletUtilities.prototype.toLeaftletLatLngBounds = function (bounds, map) {
            if (!bounds) {
                return null;
            }
            return map
                ? map.wrapLatLngBounds(new L.LatLngBounds([bounds.brLat, bounds.tlLng], [bounds.tlLat, bounds.brLng]))
                : new L.LatLngBounds([bounds.brLat, bounds.tlLng], [bounds.tlLat, bounds.brLng]);
        };
        LeafletUtilities.prototype.fromLeafletIcon = function (icon) {
            if (icon === null || icon === undefined) {
                return null;
            }
            return new VRS.MapIcon(icon.options.iconUrl, VRS.leafletUtilities.fromLeafletSize(icon.options.iconSize), VRS.leafletUtilities.fromLeafletPoint(icon.options.iconAnchor), null, null);
        };
        LeafletUtilities.prototype.toLeafletIcon = function (icon) {
            if (typeof icon === 'string') {
                return null;
            }
            return L.icon({
                iconUrl: icon.url,
                iconSize: VRS.leafletUtilities.toLeafletSize(icon.size),
                iconAnchor: VRS.leafletUtilities.toLeafletPoint(icon.anchor)
            });
        };
        LeafletUtilities.prototype.fromLeafletContent = function (content) {
            if (content === null || content === undefined) {
                return null;
            }
            else {
                if (typeof content === "string") {
                    return content;
                }
                return content.innerText;
            }
        };
        LeafletUtilities.prototype.fromLeafletSize = function (size) {
            if (size === null || size === undefined) {
                return null;
            }
            if (size instanceof L.Point) {
                return {
                    width: size.x,
                    height: size.y
                };
            }
            return {
                width: size[0],
                height: size[1]
            };
        };
        LeafletUtilities.prototype.toLeafletSize = function (size) {
            if (size === null || size === undefined) {
                return null;
            }
            return L.point(size.width, size.height);
        };
        LeafletUtilities.prototype.fromLeafletPoint = function (point) {
            if (point === null || point === undefined) {
                return null;
            }
            if (point instanceof L.Point) {
                return point;
            }
            return {
                x: point[0],
                y: point[1]
            };
        };
        LeafletUtilities.prototype.toLeafletPoint = function (point) {
            if (point === null || point === undefined) {
                return null;
            }
            if (point instanceof L.Point) {
                return point;
            }
            return L.point(point.x, point.y);
        };
        LeafletUtilities.prototype.fromLeafletMapPosition = function (mapPosition) {
            switch (mapPosition || '') {
                case 'topleft': return VRS.MapPosition.TopLeft;
                case 'bottomleft': return VRS.MapPosition.BottomLeft;
                case 'bottomright': return VRS.MapPosition.BottomRight;
                default: return VRS.MapPosition.TopRight;
            }
        };
        LeafletUtilities.prototype.toLeafletMapPosition = function (mapPosition) {
            switch (mapPosition || VRS.MapPosition.TopRight) {
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
        };
        LeafletUtilities.prototype.toLeafletMapMarker = function (mapMarker) {
            var wrapper = mapMarker;
            return wrapper ? wrapper.marker : null;
        };
        LeafletUtilities.prototype.toLeafletMapMarkers = function (mapMarkers) {
            var result = [];
            var len = mapMarkers ? mapMarkers.length : 0;
            for (var i = 0; i < len; ++i) {
                result.push(this.toLeafletMapMarker(mapMarkers[i]));
            }
            return result;
        };
        return LeafletUtilities;
    }());
    VRS.LeafletUtilities = LeafletUtilities;
    VRS.leafletUtilities = new LeafletUtilities();
    VRS.jQueryUIHelper = VRS.jQueryUIHelper || {};
    VRS.jQueryUIHelper.getMapPlugin = function (jQueryElement) {
        return jQueryElement.data('vrsVrsMap');
    };
    VRS.jQueryUIHelper.getMapOptions = function (overrides) {
        return $.extend({
            key: null,
            version: '',
            sensor: false,
            libraries: [],
            loadMarkerWithLabel: false,
            loadMarkerCluster: false,
            openOnCreate: true,
            waitUntilReady: true,
            zoom: 12,
            center: { lat: 51.5, lng: -0.125 },
            showMapTypeControl: true,
            mapTypeId: VRS.MapType.Hybrid,
            streetViewControl: VRS.globalOptions.mapShowStreetView,
            scrollwheel: VRS.globalOptions.mapScrollWheelActive,
            scaleControl: VRS.globalOptions.mapShowScaleControl,
            draggable: VRS.globalOptions.mapDraggable,
            controlStyle: VRS.MapControlStyle.Default,
            controlPosition: undefined,
            pointsOfInterest: VRS.globalOptions.mapShowPointsOfInterest,
            showHighContrast: VRS.globalOptions.mapShowHighContrastStyle,
            mapControls: [],
            afterCreate: null,
            afterOpen: null,
            name: 'default',
            useStateOnOpen: false,
            autoSaveState: false,
            useServerDefaults: false,
            __nop: null
        }, overrides);
    };
    var MapMarker = (function () {
        function MapMarker(id, mapPlugin, map, nativeMarker, markerOptions, userOptions) {
            this._eventsHooked = false;
            this.id = id;
            this.mapPlugin = mapPlugin;
            this.map = map;
            this.marker = nativeMarker;
            this.mapIcon = VRS.leafletUtilities.fromLeafletIcon(markerOptions.icon);
            this.zIndex = markerOptions.zIndexOffset;
            this.isMarkerWithLabel = !!userOptions.useMarkerWithLabel;
            this.tag = userOptions.tag;
            this.visible = !!userOptions.visible;
            if (this.isMarkerWithLabel) {
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
        MapMarker.prototype.destroy = function () {
            if (this.labelTooltip) {
                this.setLabelVisible(false);
                this.labelTooltip = null;
            }
            this.hookEvents(false);
            this.setVisible(false);
            this.mapPlugin = null;
            this.marker = null;
            this.map = null;
            this.tag = null;
        };
        MapMarker.prototype.hookEvents = function (hook) {
            if (this._eventsHooked !== hook) {
                this._eventsHooked = hook;
                if (hook)
                    this.marker.on('click', this._marker_clicked, this);
                else
                    this.marker.off('click', this._marker_clicked, this);
                if (hook)
                    this.marker.on('dragend', this._marker_dragged, this);
                else
                    this.marker.off('dragend', this._marker_dragged, this);
            }
        };
        MapMarker.prototype._marker_clicked = function (e) {
            this.mapPlugin.raiseMarkerClicked(this.id);
        };
        MapMarker.prototype._marker_dragged = function (e) {
            this.mapPlugin.raiseMarkerDragged(this.id);
        };
        MapMarker.prototype.getDraggable = function () {
            return this.marker.dragging.enabled();
        };
        MapMarker.prototype.setDraggable = function (draggable) {
            if (draggable) {
                this.marker.dragging.enable();
            }
            else {
                this.marker.dragging.disable();
            }
        };
        MapMarker.prototype.getIcon = function () {
            return this.mapIcon;
        };
        MapMarker.prototype.setIcon = function (icon) {
            this.marker.setIcon(VRS.leafletUtilities.toLeafletIcon(icon));
            this.mapIcon = icon;
        };
        MapMarker.prototype.getPosition = function () {
            return VRS.leafletUtilities.fromLeafletLatLng(this.marker.getLatLng());
        };
        MapMarker.prototype.setPosition = function (position) {
            this.marker.setLatLng(VRS.leafletUtilities.toLeafletLatLng(position, this.map));
            if (this.labelTooltip) {
                this.labelTooltip.setLatLng(this.marker.getLatLng());
            }
        };
        MapMarker.prototype.getTooltip = function () {
            var icon = this.marker.getElement();
            return icon ? icon.title : null;
        };
        MapMarker.prototype.setTooltip = function (text) {
            var icon = this.marker.getElement();
            if (icon) {
                icon.title = text;
            }
        };
        MapMarker.prototype.getVisible = function () {
            return this.visible;
        };
        MapMarker.prototype.setVisible = function (visible) {
            if (visible !== this.getVisible()) {
                if (visible) {
                    this.marker.addTo(this.map);
                }
                else {
                    this.marker.removeFrom(this.map);
                }
                this.visible = visible;
            }
        };
        MapMarker.prototype.getZIndex = function () {
            return this.zIndex;
        };
        MapMarker.prototype.setZIndex = function (zIndex) {
            this.marker.setZIndexOffset(zIndex);
            this.zIndex = zIndex;
        };
        MapMarker.prototype.getLabelVisible = function () {
            return this.labelTooltip && this.labelTooltip.isOpen();
        };
        MapMarker.prototype.setLabelVisible = function (visible) {
            if (this.labelTooltip) {
                if (visible !== this.getLabelVisible()) {
                    if (visible) {
                        this.map.openTooltip(this.labelTooltip);
                    }
                    else {
                        this.map.closeTooltip(this.labelTooltip);
                    }
                }
            }
        };
        MapMarker.prototype.getLabelContent = function () {
            return this.labelTooltip ? this.labelTooltip.getContent() : '';
        };
        MapMarker.prototype.setLabelContent = function (content) {
            if (this.labelTooltip) {
                this.labelTooltip.setContent(content);
            }
        };
        MapMarker.prototype.getLabelAnchor = function () {
            return null;
        };
        MapMarker.prototype.setLabelAnchor = function (anchor) {
            ;
        };
        return MapMarker;
    }());
    var MapPolyline = (function () {
        function MapPolyline(id, map, nativePolyline, tag, options) {
            this.id = id;
            this.map = map;
            this.polyline = nativePolyline;
            this.tag = tag;
            this.visible = options.visible;
            this._StrokeColour = options.strokeColour;
            this._StrokeOpacity = options.strokeOpacity;
            this._StrokeWeight = options.strokeWeight;
        }
        MapPolyline.prototype.destroy = function () {
            this.setVisible(false);
            this.polyline = null;
            this.map = null;
            this.tag = null;
        };
        MapPolyline.prototype.getDraggable = function () {
            return false;
        };
        MapPolyline.prototype.setDraggable = function (draggable) {
            ;
        };
        MapPolyline.prototype.getEditable = function () {
            return false;
        };
        MapPolyline.prototype.setEditable = function (editable) {
            ;
        };
        MapPolyline.prototype.getVisible = function () {
            return this.visible;
        };
        MapPolyline.prototype.setVisible = function (visible) {
            if (this.visible !== visible) {
                if (visible) {
                    this.polyline.addTo(this.map);
                }
                else {
                    this.polyline.removeFrom(this.map);
                }
                this.visible = visible;
            }
        };
        MapPolyline.prototype.getStrokeColour = function () {
            return this._StrokeColour;
        };
        MapPolyline.prototype.setStrokeColour = function (value) {
            if (value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polyline.setStyle({
                    color: value
                });
            }
        };
        MapPolyline.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapPolyline.prototype.setStrokeOpacity = function (value) {
            if (value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polyline.setStyle({
                    opacity: value
                });
            }
        };
        MapPolyline.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapPolyline.prototype.setStrokeWeight = function (value) {
            if (value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polyline.setStyle({
                    weight: value
                });
            }
        };
        MapPolyline.prototype.getPath = function () {
            return VRS.leafletUtilities.fromLeafletLatLngArray((this.polyline.getLatLngs()));
        };
        MapPolyline.prototype.setPath = function (path) {
            this.polyline.setLatLngs(VRS.leafletUtilities.toLeafletLatLngArray(path, this.map));
        };
        MapPolyline.prototype.getFirstLatLng = function () {
            var result = null;
            var nativePath = this.polyline.getLatLngs();
            if (nativePath.length)
                result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[0]);
            return result;
        };
        MapPolyline.prototype.getLastLatLng = function () {
            var result = null;
            var nativePath = this.polyline.getLatLngs();
            if (nativePath.length)
                result = VRS.leafletUtilities.fromLeafletLatLng(nativePath[nativePath.length - 1]);
            return result;
        };
        return MapPolyline;
    }());
    var MapCircle = (function () {
        function MapCircle(id, map, nativeCircle, tag, options) {
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
        MapCircle.prototype.destroy = function () {
            this.setVisible(false);
            this.circle = null;
            this.map = null;
            this.tag = null;
        };
        MapCircle.prototype.getBounds = function () {
            return VRS.leafletUtilities.fromLeafletLatLngBounds(this.circle.getBounds());
        };
        MapCircle.prototype.getCenter = function () {
            return VRS.leafletUtilities.fromLeafletLatLng(this.circle.getLatLng());
        };
        MapCircle.prototype.setCenter = function (value) {
            this.circle.setLatLng(VRS.leafletUtilities.toLeafletLatLng(value, this.map));
        };
        MapCircle.prototype.getDraggable = function () {
            return false;
        };
        MapCircle.prototype.setDraggable = function (value) {
            ;
        };
        MapCircle.prototype.getEditable = function () {
            return false;
        };
        MapCircle.prototype.setEditable = function (value) {
            ;
        };
        MapCircle.prototype.getRadius = function () {
            return this.circle.getRadius();
        };
        MapCircle.prototype.setRadius = function (value) {
            this.circle.setRadius(value);
        };
        MapCircle.prototype.getVisible = function () {
            return this.visible;
        };
        MapCircle.prototype.setVisible = function (visible) {
            if (this.visible !== visible) {
                if (visible) {
                    this.circle.addTo(this.map);
                }
                else {
                    this.circle.removeFrom(this.map);
                }
                this.visible = visible;
            }
        };
        MapCircle.prototype.getFillColor = function () {
            return this._FillColour;
        };
        MapCircle.prototype.setFillColor = function (value) {
            if (this._FillColour !== value) {
                this._FillColour = value;
                this.circle.setStyle({
                    fillColor: value
                });
            }
        };
        MapCircle.prototype.getFillOpacity = function () {
            return this._FillOpacity;
        };
        MapCircle.prototype.setFillOpacity = function (value) {
            if (this._FillOpacity !== value) {
                this._FillOpacity = value;
                this.circle.setStyle({
                    fillOpacity: value
                });
            }
        };
        MapCircle.prototype.getStrokeColor = function () {
            return this._StrokeColour;
        };
        MapCircle.prototype.setStrokeColor = function (value) {
            if (this._StrokeColour !== value) {
                this._StrokeColour = value;
                this.circle.setStyle({
                    color: value
                });
            }
        };
        MapCircle.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapCircle.prototype.setStrokeOpacity = function (value) {
            if (this._StrokeOpacity !== value) {
                this._StrokeOpacity = value;
                this.circle.setStyle({
                    opacity: value
                });
            }
        };
        MapCircle.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapCircle.prototype.setStrokeWeight = function (value) {
            if (this._StrokeWeight !== value) {
                this._StrokeWeight = value;
                this.circle.setStyle({
                    weight: value
                });
            }
        };
        MapCircle.prototype.getZIndex = function () {
            return this._ZIndex;
        };
        MapCircle.prototype.setZIndex = function (value) {
            this._ZIndex = value;
        };
        return MapCircle;
    }());
    var MapControl = (function (_super) {
        __extends(MapControl, _super);
        function MapControl(element, options) {
            var _this = _super.call(this, options) || this;
            _this.element = element;
            return _this;
        }
        MapControl.prototype.onAdd = function (map) {
            var result = $('<div class="leaflet-control"></div>');
            result.append(this.element);
            return result[0];
        };
        return MapControl;
    }(L.Control));
    var MapPolygon = (function () {
        function MapPolygon(id, nativeMap, nativePolygon, tag, options) {
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
        MapPolygon.prototype.destroy = function () {
            this.setVisible(false);
            this.map = null;
            this.polygon = null;
            this.tag = null;
        };
        MapPolygon.prototype.getDraggable = function () {
            return false;
        };
        MapPolygon.prototype.setDraggable = function (draggable) {
            ;
        };
        MapPolygon.prototype.getEditable = function () {
            return false;
        };
        MapPolygon.prototype.setEditable = function (editable) {
            ;
        };
        MapPolygon.prototype.getVisible = function () {
            return this.visible;
        };
        MapPolygon.prototype.setVisible = function (visible) {
            if (visible != this.visible) {
                if (visible) {
                    this.polygon.addTo(this.map);
                }
                else {
                    this.polygon.removeFrom(this.map);
                }
                this.visible = visible;
            }
        };
        MapPolygon.prototype.getFirstPath = function () {
            return VRS.leafletUtilities.fromLeafletLatLngArray(this.polygon.getLatLngs());
        };
        MapPolygon.prototype.setFirstPath = function (path) {
            this.polygon.setLatLngs(path);
        };
        MapPolygon.prototype.getPaths = function () {
            return [
                this.getFirstPath()
            ];
        };
        MapPolygon.prototype.setPaths = function (paths) {
            this.setFirstPath(paths[0]);
        };
        MapPolygon.prototype.getClickable = function () {
            return this.polygon.options.interactive;
        };
        MapPolygon.prototype.setClickable = function (value) {
            if (value !== this.getClickable()) {
                this.polygon.options.interactive = value;
            }
        };
        MapPolygon.prototype.getFillColour = function () {
            return this._FillColour;
        };
        MapPolygon.prototype.setFillColour = function (value) {
            if (value !== this._FillColour) {
                this._FillColour = value;
                this.polygon.setStyle({ fillColor: value });
            }
        };
        MapPolygon.prototype.getFillOpacity = function () {
            return this._FillOpacity;
        };
        MapPolygon.prototype.setFillOpacity = function (value) {
            if (value !== this._FillOpacity) {
                this._FillOpacity = value;
                this.polygon.setStyle({ fillOpacity: value });
            }
        };
        MapPolygon.prototype.getStrokeColour = function () {
            return this._StrokeColour;
        };
        MapPolygon.prototype.setStrokeColour = function (value) {
            if (value !== this._StrokeColour) {
                this._StrokeColour = value;
                this.polygon.setStyle({ color: value });
            }
        };
        MapPolygon.prototype.getStrokeOpacity = function () {
            return this._StrokeOpacity;
        };
        MapPolygon.prototype.setStrokeOpacity = function (value) {
            if (value !== this._StrokeOpacity) {
                this._StrokeOpacity = value;
                this.polygon.setStyle({ opacity: value });
            }
        };
        MapPolygon.prototype.getStrokeWeight = function () {
            return this._StrokeWeight;
        };
        MapPolygon.prototype.setStrokeWeight = function (value) {
            if (value !== this._StrokeWeight) {
                this._StrokeWeight = value;
                this.polygon.setStyle({ weight: value });
            }
        };
        MapPolygon.prototype.getZIndex = function () {
            return this._ZIndex;
        };
        MapPolygon.prototype.setZIndex = function (value) {
            if (value !== this._ZIndex) {
                this._ZIndex = value;
            }
        };
        return MapPolygon;
    }());
    var MapInfoWindow = (function () {
        function MapInfoWindow(id, nativeMap, nativeInfoWindow, tag, options) {
            this.id = id;
            this.map = nativeMap;
            this.infoWindow = nativeInfoWindow;
            this.tag = tag;
            this.isOpen = false;
            this._DisableAutoPan = options.disableAutoPan;
            this._MaxWidth = options.maxWidth;
            this._PixelOffset = options.pixelOffset;
        }
        MapInfoWindow.prototype.destroy = function () {
            this.infoWindow.setContent('');
            this.map = null;
            this.tag = null;
            this.infoWindow = null;
        };
        MapInfoWindow.prototype.getContent = function () {
            return this.infoWindow.getContent();
        };
        MapInfoWindow.prototype.setContent = function (value) {
            this.infoWindow.setContent(value);
        };
        MapInfoWindow.prototype.getDisableAutoPan = function () {
            return this._DisableAutoPan;
        };
        MapInfoWindow.prototype.setDisableAutoPan = function (value) {
            if (this._DisableAutoPan !== value) {
                this._DisableAutoPan = value;
                this.infoWindow.options.autoPan = !value;
            }
        };
        MapInfoWindow.prototype.getMaxWidth = function () {
            return this._MaxWidth;
        };
        MapInfoWindow.prototype.setMaxWidth = function (value) {
            if (this._MaxWidth !== value) {
                this._MaxWidth = value;
                this.infoWindow.options.maxWidth = value;
            }
        };
        MapInfoWindow.prototype.getPixelOffset = function () {
            return this._PixelOffset;
        };
        MapInfoWindow.prototype.setPixelOffset = function (value) {
            if (this._PixelOffset !== value) {
                this._PixelOffset = value;
                this.infoWindow.options.offset = VRS.leafletUtilities.toLeafletSize(value);
            }
        };
        MapInfoWindow.prototype.getPosition = function () {
            return VRS.leafletUtilities.fromLeafletLatLng(this.infoWindow.getLatLng());
        };
        MapInfoWindow.prototype.setPosition = function (value) {
            this.infoWindow.setLatLng(VRS.leafletUtilities.toLeafletLatLng(value, this.map));
        };
        MapInfoWindow.prototype.getZIndex = function () {
            return 1;
        };
        MapInfoWindow.prototype.setZIndex = function (value) {
            ;
        };
        MapInfoWindow.prototype.open = function (mapMarker) {
            this.close();
            if (!this.isOpen) {
                this.isOpen = true;
                if (!mapMarker) {
                    this.map.openPopup(this.infoWindow);
                }
                else {
                    this.boundMarker = mapMarker;
                    var markerHeight = mapMarker.getIcon().size.height;
                    this.setPixelOffset({ width: 0, height: -markerHeight });
                    mapMarker.marker.bindPopup(this.infoWindow).openPopup();
                }
            }
        };
        MapInfoWindow.prototype.close = function () {
            if (this.isOpen) {
                this.isOpen = false;
                if (!this.boundMarker) {
                    this.map.closePopup(this.infoWindow);
                }
                else {
                    if (this.boundMarker.marker) {
                        this.boundMarker.marker.closePopup();
                        this.boundMarker.marker.unbindPopup();
                    }
                    this.boundMarker = null;
                }
            }
        };
        return MapInfoWindow;
    }());
    var MapMarkerClusterer = (function () {
        function MapMarkerClusterer(nativeMap, settings) {
            this.map = nativeMap;
            this.maxZoom = settings.maxZoom;
            this.createClusterGroup();
        }
        MapMarkerClusterer.prototype.createClusterGroup = function () {
            var mapMarkers = null;
            if (this.clusterGroup) {
                mapMarkers = this.clusterGroup.getLayers();
                this.destroyClusterGroup();
            }
            var options = {
                disableClusteringAtZoom: this.maxZoom + 1,
                spiderfyOnMaxZoom: false,
                showCoverageOnHover: true
            };
            this.clusterGroup = L.markerClusterGroup(options);
            if (mapMarkers && mapMarkers.length > 0) {
                this.clusterGroup.addLayers(mapMarkers);
            }
            this.map.addLayer(this.clusterGroup);
        };
        MapMarkerClusterer.prototype.destroyClusterGroup = function () {
            if (this.clusterGroup) {
                this.clusterGroup.removeLayers(this.clusterGroup.getLayers());
                this.map.removeLayer(this.clusterGroup);
                this.clusterGroup = null;
            }
        };
        MapMarkerClusterer.prototype.getNative = function () {
            return this.clusterGroup;
        };
        MapMarkerClusterer.prototype.getNativeType = function () {
            return 'OpenStreetMap';
        };
        MapMarkerClusterer.prototype.getMaxZoom = function () {
            return this.maxZoom;
        };
        MapMarkerClusterer.prototype.setMaxZoom = function (maxZoom) {
            if (maxZoom !== this.maxZoom) {
                this.maxZoom = maxZoom;
                this.createClusterGroup();
            }
        };
        MapMarkerClusterer.prototype.addMarker = function (marker, noRepaint) {
            if (marker) {
                var nativeMarker = VRS.leafletUtilities.toLeafletMapMarker(marker);
                nativeMarker.remove();
                this.clusterGroup.addLayer(nativeMarker);
            }
        };
        MapMarkerClusterer.prototype.addMarkers = function (markers, noRepaint) {
            if (markers) {
                var nativeMarkers = VRS.leafletUtilities.toLeafletMapMarkers(markers);
                var len = nativeMarkers.length;
                for (var i = 0; i < len; ++i) {
                    nativeMarkers[i].remove();
                }
                this.clusterGroup.addLayers(nativeMarkers);
            }
        };
        MapMarkerClusterer.prototype.removeMarker = function (marker, noRepaint) {
            if (marker) {
                this.clusterGroup.removeLayer(VRS.leafletUtilities.toLeafletMapMarker(marker));
            }
        };
        MapMarkerClusterer.prototype.removeMarkers = function (markers, noRepaint) {
            if (markers) {
                this.clusterGroup.removeLayers(VRS.leafletUtilities.toLeafletMapMarkers(markers));
            }
        };
        MapMarkerClusterer.prototype.repaint = function () {
            this.clusterGroup.refreshClusters();
        };
        return MapMarkerClusterer;
    }());
    var MapLayer = (function () {
        function MapLayer(nativeMap, nativeLayer) {
            this.map = nativeMap;
            this.layer = nativeLayer;
        }
        MapLayer.prototype.destroy = function () {
            this.map = null;
            this.layer = null;
        };
        MapLayer.prototype.getOpacity = function () {
            return this.layer.options.opacity;
        };
        MapLayer.prototype.setOpacity = function (value) {
            this.layer.setOpacity(value);
        };
        return MapLayer;
    }());
    var MapPluginState = (function () {
        function MapPluginState() {
            this.mapContainer = undefined;
            this.map = undefined;
            this.mapName = undefined;
            this.tileLayer = undefined;
            this.markers = {};
            this.polylines = {};
            this.circles = {};
            this.polygons = {};
            this.infoWindows = {};
            this.layers = {};
            this.eventsHooked = false;
            this.settingCenter = undefined;
            this.defaultBrightness = undefined;
            this.brightness = 100;
        }
        return MapPluginState;
    }());
    var MapPlugin = (function (_super) {
        __extends(MapPlugin, _super);
        function MapPlugin() {
            var _this = _super.call(this) || this;
            _this._EventPluginName = 'vrsMap';
            _this.options = VRS.jQueryUIHelper.getMapOptions();
            return _this;
        }
        MapPlugin.prototype._getState = function () {
            var result = this.element.data('mapPluginState');
            if (result === undefined) {
                result = new MapPluginState();
                this.element.data('mapPluginState', result);
            }
            return result;
        };
        MapPlugin.prototype._create = function () {
            if (this.options.useServerDefaults && VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if (config) {
                    this.options.center = { lat: config.InitialLatitude, lng: config.InitialLongitude };
                    this.options.mapTypeId = config.InitialMapType;
                    this.options.zoom = config.InitialZoom;
                }
            }
            var state = this._getState();
            state.mapContainer = $('<div />')
                .addClass('vrsMap')
                .appendTo(this.element);
            if (VRS.refreshManager) {
                VRS.refreshManager.registerTarget(this.element, this._targetResized, this);
            }
            if (this.options.afterCreate) {
                this.options.afterCreate(this);
            }
            if (this.options.openOnCreate) {
                this.open();
            }
        };
        MapPlugin.prototype._destroy = function () {
            var state = this._getState();
            this._hookEvents(state, false);
            if (VRS.refreshManager)
                VRS.refreshManager.unregisterTarget(this.element);
            if (state.mapContainer)
                state.mapContainer.remove();
        };
        MapPlugin.prototype._hookEvents = function (state, hook) {
            if (state.map) {
                if ((hook && !state.eventsHooked) || (!hook && state.eventsHooked)) {
                    state.eventsHooked = hook;
                    if (hook)
                        state.map.on('resize', this._map_resized, this);
                    else
                        state.map.off('resize', this._map_resized, this);
                    if (hook)
                        state.map.on('move', this._map_moved, this);
                    else
                        state.map.off('move', this._map_moved, this);
                    if (hook)
                        state.map.on('moveend', this._map_moveEnded, this);
                    else
                        state.map.off('moveend', this._map_moveEnded, this);
                    if (hook)
                        state.map.on('click', this._map_clicked, this);
                    else
                        state.map.off('click', this._map_clicked, this);
                    if (hook)
                        state.map.on('dblclick', this._map_doubleClicked, this);
                    else
                        state.map.off('dblclick', this._map_doubleClicked, this);
                    if (hook)
                        state.map.on('zoomend', this._map_zoomEnded, this);
                    else
                        state.map.off('zoomend', this._map_zoomEnded, this);
                    if (hook)
                        state.tileLayer.on('load', this._tileLayer_loaded, this);
                    else
                        state.tileLayer.off('load', this._tileLayer_loaded, this);
                }
            }
        };
        MapPlugin.prototype._map_resized = function (e) {
            this._raiseBoundsChanged();
        };
        MapPlugin.prototype._map_moved = function (e) {
            this._raiseCenterChanged();
        };
        MapPlugin.prototype._map_moveEnded = function (e) {
            this._onIdle();
        };
        MapPlugin.prototype._map_clicked = function (e) {
            this._userNotIdle();
            this._raiseClicked(e);
        };
        MapPlugin.prototype._map_doubleClicked = function (e) {
            this._userNotIdle();
            this._raiseDoubleClicked(e);
        };
        MapPlugin.prototype._map_zoomEnded = function (e) {
            this._raiseZoomChanged();
            this._onIdle();
        };
        MapPlugin.prototype._tileLayer_loaded = function (e) {
            this._raiseTilesLoaded();
        };
        MapPlugin.prototype.getNative = function () {
            return this._getState().map;
        };
        MapPlugin.prototype.getNativeType = function () {
            return 'OpenStreetMap';
        };
        MapPlugin.prototype.isOpen = function () {
            return !!this._getState().map;
        };
        MapPlugin.prototype.isReady = function () {
            var state = this._getState();
            return !!state.map;
        };
        MapPlugin.prototype.getBounds = function () {
            return this._getBounds(this._getState());
        };
        MapPlugin.prototype._getBounds = function (state) {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLngBounds(state.map.getBounds()) : { tlLat: 0, tlLng: 0, brLat: 0, brLng: 0 };
        };
        MapPlugin.prototype.getCenter = function () {
            return this._getCenter(this._getState());
        };
        MapPlugin.prototype._getCenter = function (state) {
            return state.map ? VRS.leafletUtilities.fromLeafletLatLng(state.map.getCenter()) : this.options.center;
        };
        MapPlugin.prototype.setCenter = function (latLng) {
            this._setCenter(this._getState(), latLng);
        };
        MapPlugin.prototype._setCenter = function (state, latLng) {
            if (state.settingCenter === undefined || state.settingCenter === null || state.settingCenter.lat != latLng.lat || state.settingCenter.lng != latLng.lng) {
                try {
                    state.settingCenter = latLng;
                    if (state.map)
                        state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(latLng, state.map));
                    else
                        this.options.center = latLng;
                }
                finally {
                    state.settingCenter = undefined;
                }
            }
        };
        MapPlugin.prototype.getDraggable = function () {
            return this.options.draggable;
        };
        MapPlugin.prototype.getMapType = function () {
            return this._getMapType(this._getState());
        };
        MapPlugin.prototype._getMapType = function (state) {
            return this.options.mapTypeId;
        };
        MapPlugin.prototype.setMapType = function (mapType) {
            this._setMapType(this._getState(), mapType);
        };
        MapPlugin.prototype._setMapType = function (state, mapType) {
            this.options.mapTypeId = mapType;
        };
        MapPlugin.prototype.getScrollWheel = function () {
            return this.options.scrollwheel;
        };
        MapPlugin.prototype.getStreetView = function () {
            return this.options.streetViewControl;
        };
        MapPlugin.prototype.getZoom = function () {
            return this._getZoom(this._getState());
        };
        MapPlugin.prototype._getZoom = function (state) {
            return state.map ? state.map.getZoom() : this.options.zoom;
        };
        MapPlugin.prototype.setZoom = function (zoom) {
            this._setZoom(this._getState(), zoom);
        };
        MapPlugin.prototype._setZoom = function (state, zoom) {
            if (state.map)
                state.map.setZoom(zoom);
            else
                this.options.zoom = zoom;
        };
        MapPlugin.prototype.unhook = function (hookResult) {
            VRS.globalDispatch.unhookJQueryUIPluginEvent(this.element, hookResult);
        };
        MapPlugin.prototype.hookBoundsChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'boundsChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseBoundsChanged = function () {
            this._trigger('boundsChanged');
        };
        MapPlugin.prototype.hookBrightnessChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'brightnessChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseBrightnessChanged = function () {
            this._trigger('brightnessChanged');
        };
        MapPlugin.prototype.hookCenterChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'centerChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseCenterChanged = function () {
            this._trigger('centerChanged');
        };
        MapPlugin.prototype.hookClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'clicked', callback, forceThis);
        };
        MapPlugin.prototype._raiseClicked = function (mouseEvent) {
            this._trigger('clicked', null, { mouseEvent: mouseEvent });
        };
        MapPlugin.prototype.hookDoubleClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'doubleClicked', callback, forceThis);
        };
        MapPlugin.prototype._raiseDoubleClicked = function (mouseEvent) {
            this._trigger('doubleClicked', null, { mouseEvent: mouseEvent });
        };
        MapPlugin.prototype.hookIdle = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'idle', callback, forceThis);
        };
        MapPlugin.prototype._raiseIdle = function () {
            this._trigger('idle');
        };
        MapPlugin.prototype.hookMapTypeChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mapTypeChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseMapTypeChanged = function () {
            this._trigger('mapTypeChanged');
        };
        MapPlugin.prototype.hookRightClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'mouseEvent', callback, forceThis);
        };
        MapPlugin.prototype._raiseRightClicked = function (mouseEvent) {
            this._trigger('mouseEvent', null, { mouseEvent: mouseEvent });
        };
        MapPlugin.prototype.hookTilesLoaded = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'tilesLoaded', callback, forceThis);
        };
        MapPlugin.prototype._raiseTilesLoaded = function () {
            this._trigger('tilesLoaded');
        };
        MapPlugin.prototype.hookZoomChanged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'zoomChanged', callback, forceThis);
        };
        MapPlugin.prototype._raiseZoomChanged = function () {
            this._trigger('zoomChanged');
        };
        MapPlugin.prototype.hookMarkerClicked = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerClicked', callback, forceThis);
        };
        MapPlugin.prototype.raiseMarkerClicked = function (id) {
            this._trigger('markerClicked', null, { id: id });
        };
        MapPlugin.prototype.hookMarkerDragged = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, this._EventPluginName, 'markerDragged', callback, forceThis);
        };
        MapPlugin.prototype.raiseMarkerDragged = function (id) {
            this._trigger('markerDragged', null, { id: id });
        };
        MapPlugin.prototype.hookInfoWindowClosedByUser = function (callback, forceThis) {
            return VRS.globalDispatch.hookJQueryUIPluginEvent(this.element, 'vrsMap', 'infoWindowClosedByUser', callback, forceThis);
        };
        MapPlugin.prototype._raiseInfoWindowClosedByUser = function (id) {
            this._trigger('infoWindowClosedByUser', null, { id: id });
        };
        MapPlugin.prototype._onIdle = function () {
            if (this.options.autoSaveState)
                this.saveState();
            this._raiseIdle();
        };
        MapPlugin.prototype._onMapTypeChanged = function () {
            if (this.options.autoSaveState)
                this.saveState();
            this._raiseMapTypeChanged();
        };
        MapPlugin.prototype.open = function (userOptions) {
            var _this = this;
            var tileServerSettings = VRS.serverConfig.get().TileServerSettings;
            if (tileServerSettings) {
                var mapOptions = $.extend({}, userOptions, {
                    zoom: this.options.zoom,
                    center: this.options.center,
                    mapTypeControl: this.options.showMapTypeControl,
                    mapTypeId: this.options.mapTypeId,
                    streetViewControl: this.options.streetViewControl,
                    scrollwheel: this.options.scrollwheel,
                    scaleControl: this.options.scaleControl,
                    draggable: this.options.draggable,
                    showHighContrast: this.options.showHighContrast,
                    controlStyle: this.options.controlStyle,
                    controlPosition: this.options.controlPosition,
                    mapControls: this.options.mapControls
                });
                var overrideBrightness = tileServerSettings.DefaultBrightness;
                if (this.options.useStateOnOpen) {
                    var settings = this.loadState();
                    mapOptions.zoom = settings.zoom;
                    mapOptions.center = settings.center;
                    mapOptions.mapTypeId = settings.mapTypeId;
                    if (tileServerSettings.Name === settings.brightnessMapName && settings.brightness) {
                        overrideBrightness = settings.brightness;
                    }
                }
                mapOptions.zoom = this._normaliseZoom(mapOptions.zoom, tileServerSettings.MinZoom, tileServerSettings.MaxZoom);
                var leafletOptions = {
                    attributionControl: true,
                    zoom: mapOptions.zoom,
                    center: VRS.leafletUtilities.toLeafletLatLng(mapOptions.center, null),
                    scrollWheelZoom: mapOptions.scrollwheel,
                    dragging: mapOptions.draggable,
                    zoomControl: mapOptions.scaleControl
                };
                var state = this._getState();
                state.map = L.map(state.mapContainer[0], leafletOptions);
                state.mapName = tileServerSettings.Name;
                state.defaultBrightness = tileServerSettings.DefaultBrightness;
                var tileServerOptions = this._buildTileServerOptions(state, tileServerSettings, overrideBrightness);
                state.tileLayer = L.tileLayer(tileServerSettings.Url, tileServerOptions);
                state.tileLayer.addTo(state.map);
                this._hookEvents(state, true);
                if (mapOptions.mapControls) {
                    $.each(mapOptions.mapControls, function (idx, ctrl) {
                        _this.addControl(ctrl.control, ctrl.position);
                    });
                }
                var waitUntilReady = function () {
                    if (_this.options.waitUntilReady && !_this.isReady()) {
                        setTimeout(waitUntilReady, 100);
                    }
                    else {
                        if (_this.options.afterOpen)
                            _this.options.afterOpen(_this);
                    }
                };
                waitUntilReady();
            }
        };
        MapPlugin.prototype._buildTileServerOptions = function (state, settings, overrideBrightness) {
            var result = {
                attribution: this._attributionToHtml(settings.Attribution),
                detectRetina: settings.DetectRetina,
                zoomReverse: settings.ZoomReverse,
            };
            if (VRS.globalOptions.mapLeafletNoWrap) {
                result.noWrap = true;
            }
            if (settings.ClassName) {
                result.className = settings.ClassName;
            }
            if (!settings.IsLayer) {
                var brightness = overrideBrightness && !isNaN(overrideBrightness) ? overrideBrightness : settings.DefaultBrightness;
                result.className = this._setBrightnessClass(result.className, state.brightness, brightness);
                state.brightness = brightness;
            }
            else {
                if (settings.DefaultOpacity > 0 && settings.DefaultOpacity < 100) {
                    result.opacity = settings.DefaultOpacity / 100;
                }
            }
            if (settings.MaxNativeZoom !== null && settings.MaxNativeZoom !== undefined) {
                result.maxNativeZoom = settings.MaxNativeZoom;
            }
            if (settings.MinNativeZoom !== null && settings.MinNativeZoom !== undefined) {
                result.minNativeZoom = settings.MinNativeZoom;
            }
            if (settings.MaxZoom !== null && settings.MaxZoom !== undefined) {
                result.maxZoom = settings.MaxZoom;
            }
            if (settings.MinZoom !== null && settings.MinZoom !== undefined) {
                result.minZoom = settings.MinZoom;
            }
            if (settings.Subdomains !== null && settings.Subdomains !== undefined) {
                result.subdomains = settings.Subdomains;
            }
            if (settings.ZoomOffset !== null && settings.ZoomOffset !== undefined) {
                result.zoomOffset = settings.ZoomOffset;
            }
            if (settings.IsTms) {
                result.tms = true;
            }
            if (settings.ErrorTileUrl != null) {
                result.errorTileUrl = settings.ErrorTileUrl;
            }
            var countExpandos = settings.ExpandoOptions === null || settings.ExpandoOptions === undefined ? 0 : settings.ExpandoOptions.length;
            for (var i = 0; i < countExpandos; ++i) {
                var expando = settings.ExpandoOptions[i];
                result[expando.Option] = VRS.stringUtility.htmlEscape(expando.Value);
            }
            return result;
        };
        MapPlugin.prototype._attributionToHtml = function (attribution) {
            var result = VRS.stringUtility.htmlEscape(attribution);
            result = result.replace(/\[c\]/g, '&copy;');
            result = result.replace(/\[\/a\]/g, '</a>');
            result = result.replace(/\[a href=(.+?)\]/g, '<a href="$1">');
            return result;
        };
        MapPlugin.prototype._normaliseZoom = function (zoom, minZoom, maxZoom) {
            var result = zoom;
            if (result !== undefined && result !== null) {
                if (minZoom !== undefined && minZoom !== null && result < minZoom) {
                    result = minZoom;
                }
                if (maxZoom !== undefined && maxZoom !== null && result > maxZoom) {
                    result = maxZoom;
                }
            }
            return result;
        };
        MapPlugin.prototype._userNotIdle = function () {
            if (VRS.timeoutManager)
                VRS.timeoutManager.resetTimer();
        };
        MapPlugin.prototype.refreshMap = function () {
            var state = this._getState();
            if (state.map) {
                state.map.invalidateSize();
            }
        };
        MapPlugin.prototype.panTo = function (mapCenter) {
            this._panTo(mapCenter, this._getState());
        };
        MapPlugin.prototype._panTo = function (mapCenter, state) {
            if (state.settingCenter === undefined || state.settingCenter === null || state.settingCenter.lat != mapCenter.lat || state.settingCenter.lng != mapCenter.lng) {
                try {
                    state.settingCenter = mapCenter;
                    if (state.map)
                        state.map.panTo(VRS.leafletUtilities.toLeafletLatLng(mapCenter, state.map));
                    else
                        this.options.center = mapCenter;
                }
                finally {
                    state.settingCenter = undefined;
                }
            }
        };
        MapPlugin.prototype.fitBounds = function (bounds) {
            var state = this._getState();
            if (state.map) {
                state.map.fitBounds(VRS.leafletUtilities.toLeaftletLatLngBounds(bounds, state.map));
            }
        };
        MapPlugin.prototype.saveState = function () {
            var settings = this._createSettings();
            VRS.configStorage.save(this._persistenceKey(), settings);
        };
        MapPlugin.prototype.loadState = function () {
            var savedSettings = VRS.configStorage.load(this._persistenceKey(), {});
            return $.extend(this._createSettings(), savedSettings);
        };
        MapPlugin.prototype.applyState = function (config) {
            config = config || {};
            var state = this._getState();
            if (config.center)
                this._setCenter(state, config.center);
            if (config.zoom || config.zoom === 0)
                this._setZoom(state, config.zoom);
            if (config.mapTypeId)
                this._setMapType(state, config.mapTypeId);
            if (state.mapName === config.brightnessMapName) {
                this.setMapBrightness(config.brightness);
            }
        };
        ;
        MapPlugin.prototype.loadAndApplyState = function () {
            this.applyState(this.loadState());
        };
        MapPlugin.prototype._persistenceKey = function () {
            return 'vrsMapState-' + (this.options.name || 'default');
        };
        MapPlugin.prototype._createSettings = function () {
            var state = this._getState();
            return {
                zoom: this._getZoom(state),
                mapTypeId: this._getMapType(state),
                center: this._getCenter(state),
                brightnessMapName: state.mapName,
                brightness: state.brightness === state.defaultBrightness ? 0 : state.brightness
            };
        };
        MapPlugin.prototype.addMarker = function (id, userOptions) {
            var result;
            var state = this._getState();
            if (state.map) {
                if (userOptions.zIndex === null || userOptions.zIndex === undefined) {
                    userOptions.zIndex = 0;
                }
                if (userOptions.draggable) {
                    userOptions.clickable = true;
                }
                var leafletOptions = {
                    interactive: userOptions.clickable !== undefined ? userOptions.clickable : true,
                    draggable: userOptions.draggable !== undefined ? userOptions.draggable : false,
                    zIndexOffset: userOptions.zIndex,
                };
                if (userOptions.icon) {
                    leafletOptions.icon = VRS.leafletUtilities.toLeafletIcon(userOptions.icon);
                }
                if (userOptions.tooltip) {
                    leafletOptions.title = userOptions.tooltip;
                }
                var position = userOptions.position ? VRS.leafletUtilities.toLeafletLatLng(userOptions.position, state.map) : state.map.getCenter();
                this.destroyMarker(id);
                var nativeMarker = L.marker(position, leafletOptions);
                if (userOptions.visible) {
                    nativeMarker.addTo(state.map);
                }
                result = new MapMarker(id, this, state.map, nativeMarker, leafletOptions, userOptions);
                state.markers[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getMarker = function (idOrMarker) {
            if (idOrMarker instanceof MapMarker)
                return idOrMarker;
            var state = this._getState();
            return state.markers[idOrMarker];
        };
        MapPlugin.prototype.destroyMarker = function (idOrMarker) {
            var state = this._getState();
            var marker = this.getMarker(idOrMarker);
            if (marker) {
                marker.destroy();
                delete state.markers[marker.id];
                marker.id = null;
            }
        };
        MapPlugin.prototype.centerOnMarker = function (idOrMarker) {
            var state = this._getState();
            var marker = this.getMarker(idOrMarker);
            if (marker) {
                this.setCenter(marker.getPosition());
            }
        };
        MapPlugin.prototype.createMapMarkerClusterer = function (settings) {
            var result = null;
            var state = this._getState();
            if (state.map) {
                var options = $.extend({}, settings, {});
            }
            return result;
        };
        MapPlugin.prototype.addPolyline = function (id, userOptions) {
            var result;
            var state = this._getState();
            if (state.map) {
                var options = $.extend({}, userOptions, {
                    visible: true
                });
                var leafletOptions = {
                    color: options.strokeColour || '#000000'
                };
                if (options.strokeOpacity || leafletOptions.opacity === 0)
                    leafletOptions.opacity = options.strokeOpacity;
                if (options.strokeWeight || leafletOptions.weight === 0)
                    leafletOptions.weight = options.strokeWeight;
                var path = [];
                if (options.path)
                    path = VRS.leafletUtilities.toLeafletLatLngArray(options.path, state.map);
                this.destroyPolyline(id);
                var polyline = L.polyline(path, leafletOptions);
                if (options.visible) {
                    polyline.addTo(state.map);
                }
                result = new MapPolyline(id, state.map, polyline, options.tag, {
                    strokeColour: options.strokeColour,
                    strokeOpacity: options.strokeOpacity,
                    strokeWeight: options.strokeWeight
                });
                state.polylines[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getPolyline = function (idOrPolyline) {
            if (idOrPolyline instanceof MapPolyline)
                return idOrPolyline;
            var state = this._getState();
            return state.polylines[idOrPolyline];
        };
        MapPlugin.prototype.destroyPolyline = function (idOrPolyline) {
            var state = this._getState();
            var polyline = this.getPolyline(idOrPolyline);
            if (polyline) {
                polyline.destroy();
                delete state.polylines[polyline.id];
                polyline.id = null;
            }
        };
        MapPlugin.prototype.trimPolyline = function (idOrPolyline, countPoints, fromStart) {
            var emptied = false;
            var countRemoved = 0;
            if (countPoints > 0) {
                var polyline = this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getLatLngs();
                var length = points.length;
                if (length < countPoints)
                    countPoints = length;
                if (countPoints > 0) {
                    countRemoved = countPoints;
                    emptied = countPoints === length;
                    if (emptied) {
                        points = [];
                    }
                    else {
                        if (fromStart) {
                            points.splice(0, countPoints);
                        }
                        else {
                            points.splice(length - countPoints, countPoints);
                        }
                    }
                    polyline.polyline.setLatLngs(points);
                }
            }
            return { emptied: emptied, countRemoved: countRemoved };
        };
        MapPlugin.prototype.removePolylinePointAt = function (idOrPolyline, index) {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getLatLngs();
            points.splice(index, 1);
            polyline.polyline.setLatLngs(points);
        };
        MapPlugin.prototype.appendToPolyline = function (idOrPolyline, path, toStart) {
            var length = !path ? 0 : path.length;
            if (length > 0) {
                var state = this._getState();
                var polyline = this.getPolyline(idOrPolyline);
                var points = polyline.polyline.getLatLngs();
                var insertAt = toStart ? 0 : -1;
                for (var i = 0; i < length; ++i) {
                    var leafletPoint = VRS.leafletUtilities.toLeafletLatLng(path[i], state.map);
                    if (toStart) {
                        points.splice(insertAt++, 0, leafletPoint);
                    }
                    else {
                        points.push(leafletPoint);
                    }
                }
                polyline.polyline.setLatLngs(points);
            }
        };
        MapPlugin.prototype.replacePolylinePointAt = function (idOrPolyline, index, point) {
            var polyline = this.getPolyline(idOrPolyline);
            var points = polyline.polyline.getLatLngs();
            var length = points.length;
            if (index === -1)
                index = length - 1;
            if (index >= 0 && index < length) {
                var state = this._getState();
                points.splice(index, 1, VRS.leafletUtilities.toLeafletLatLng(point, state.map));
                polyline.polyline.setLatLngs(points);
            }
        };
        MapPlugin.prototype.addPolygon = function (id, userOptions) {
            var result;
            var state = this._getState();
            if (state.map) {
                var options = $.extend({}, userOptions, {
                    visible: true
                });
                var leafletOptions = {
                    color: options.strokeColour || '#000000',
                    fillColor: options.fillColour || '#ffffff',
                };
                if (options.strokeOpacity || leafletOptions.opacity === 0)
                    leafletOptions.opacity = options.strokeOpacity;
                if (options.fillOpacity || leafletOptions.fillOpacity === 0)
                    leafletOptions.fillOpacity = options.fillOpacity;
                if (options.strokeWeight || leafletOptions.weight === 0)
                    leafletOptions.weight = options.strokeWeight;
                var paths = [];
                if (options.paths)
                    paths = VRS.leafletUtilities.toLeafletLatLngArray(options.paths[0], state.map);
                this.destroyPolygon(id);
                var polygon = new L.Polygon(paths, leafletOptions);
                if (options.visible) {
                    polygon.addTo(state.map);
                }
                result = new MapPolygon(id, state.map, polygon, userOptions.tag, userOptions);
                state.polygons[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getPolygon = function (idOrPolygon) {
            if (idOrPolygon instanceof MapPolygon)
                return idOrPolygon;
            var state = this._getState();
            return state.polygons[idOrPolygon];
        };
        MapPlugin.prototype.destroyPolygon = function (idOrPolygon) {
            var state = this._getState();
            var polygon = this.getPolygon(idOrPolygon);
            if (polygon) {
                polygon.destroy();
                delete state.polygons[polygon.id];
                polygon.id = null;
            }
        };
        MapPlugin.prototype.addCircle = function (id, userOptions) {
            var result = null;
            var state = this._getState();
            if (state.map) {
                var options = $.extend({}, userOptions, {
                    visible: true
                });
                var leafletOptions = {
                    fillColor: options.fillColor || '#000',
                    fillOpacity: options.fillOpacity !== null && options.fillOpacity !== undefined ? options.fillOpacity : 0,
                    color: options.strokeColor || '#000',
                    opacity: options.strokeOpacity !== null && options.strokeOpacity !== undefined ? options.strokeOpacity : 1,
                    weight: options.strokeWeight !== null && options.strokeWeight !== undefined ? options.strokeWeight : 1,
                    radius: options.radius || 0
                };
                var centre = VRS.leafletUtilities.toLeafletLatLng(options.center, state.map);
                this.destroyCircle(id);
                var circle = L.circle(centre, leafletOptions);
                if (options.visible) {
                    circle.addTo(state.map);
                }
                result = new MapCircle(id, state.map, circle, options.tag, options);
                state.circles[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getCircle = function (idOrCircle) {
            if (idOrCircle instanceof MapCircle)
                return idOrCircle;
            var state = this._getState();
            return state.circles[idOrCircle];
        };
        MapPlugin.prototype.destroyCircle = function (idOrCircle) {
            var state = this._getState();
            var circle = this.getCircle(idOrCircle);
            if (circle) {
                circle.destroy();
                delete state.circles[circle.id];
                circle.id = null;
            }
        };
        MapPlugin.prototype.getUnusedInfoWindowId = function () {
            var result;
            var state = this._getState();
            for (var i = 1; i > 0; ++i) {
                result = 'autoID' + i;
                if (!state.infoWindows[result])
                    break;
            }
            return result;
        };
        MapPlugin.prototype.addInfoWindow = function (id, userOptions) {
            var result = null;
            var state = this._getState();
            if (state.map) {
                var options = $.extend({
                    visible: true
                }, userOptions);
                var leafletOptions = {
                    autoPan: !!!options.disableAutoPan,
                    autoClose: false,
                    closeOnClick: false,
                    maxWidth: options.maxWidth
                };
                if (options.pixelOffset) {
                    leafletOptions.offset = VRS.leafletUtilities.toLeafletSize(options.pixelOffset);
                }
                this.destroyInfoWindow(id);
                var infoWindow = new L.Popup(leafletOptions);
                if (options.position) {
                    infoWindow.setLatLng(VRS.leafletUtilities.toLeafletLatLng(options.position, state.map));
                }
                if (options.content) {
                    infoWindow.setContent(options.content);
                }
                result = new MapInfoWindow(id, state.map, infoWindow, options.tag, options);
                state.infoWindows[id] = result;
            }
            return result;
        };
        MapPlugin.prototype.getInfoWindow = function (idOrInfoWindow) {
            if (idOrInfoWindow instanceof MapInfoWindow)
                return idOrInfoWindow;
            var state = this._getState();
            return state.infoWindows[idOrInfoWindow];
        };
        MapPlugin.prototype.destroyInfoWindow = function (idOrInfoWindow) {
            var state = this._getState();
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            if (infoWindow) {
                this.closeInfoWindow(infoWindow);
                infoWindow.destroy();
                delete state.infoWindows[infoWindow.id];
                infoWindow.id = null;
            }
        };
        MapPlugin.prototype.openInfoWindow = function (idOrInfoWindow, mapMarker) {
            var state = this._getState();
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            if (infoWindow && state.map) {
                infoWindow.open(mapMarker);
            }
        };
        MapPlugin.prototype.closeInfoWindow = function (idOrInfoWindow) {
            var infoWindow = this.getInfoWindow(idOrInfoWindow);
            infoWindow.close();
        };
        MapPlugin.prototype.addControl = function (element, mapPosition) {
            var state = this._getState();
            if (state.map) {
                var controlOptions = {
                    position: VRS.leafletUtilities.toLeafletMapPosition(mapPosition)
                };
                var control = new MapControl(element, controlOptions);
                control.addTo(state.map);
            }
        };
        MapPlugin.prototype.addLayer = function (layerTileSettings, opacity) {
            var state = this._getState();
            if (state.map && layerTileSettings && layerTileSettings.IsLayer && layerTileSettings.Name) {
                var mapLayer = state.layers[layerTileSettings.Name];
                if (!mapLayer) {
                    var layerOptions = this._buildTileServerOptions(state, layerTileSettings, 100);
                    if (opacity !== null && opacity !== undefined) {
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
        };
        MapPlugin.prototype.destroyLayer = function (layerName) {
            var state = this._getState();
            if (state.map && layerName) {
                var mapLayer = state.layers[layerName];
                if (mapLayer) {
                    mapLayer.layer.removeFrom(state.map);
                    mapLayer.destroy();
                    delete state.layers[layerName];
                }
            }
        };
        MapPlugin.prototype.hasLayer = function (layerName) {
            var state = this._getState();
            return !!(state.map && layerName && state.layers[layerName]);
        };
        MapPlugin.prototype.getLayerOpacity = function (layerName) {
            var result = undefined;
            var state = this._getState();
            if (state.map && layerName) {
                var mapLayer = state.layers[layerName];
                if (mapLayer) {
                    result = mapLayer.getOpacity() * 100.0;
                }
            }
            return result;
        };
        MapPlugin.prototype.setLayerOpacity = function (layerName, opacity) {
            var state = this._getState();
            if (state.map && layerName && !isNaN(opacity)) {
                var mapLayer = state.layers[layerName];
                if (mapLayer) {
                    mapLayer.setOpacity(Math.min(1.0, Math.max(0.0, opacity / 100.0)));
                }
            }
        };
        MapPlugin.prototype.getCanSetMapBrightness = function () {
            return true;
        };
        MapPlugin.prototype.getDefaultMapBrightness = function () {
            var state = this._getState();
            return state.defaultBrightness;
        };
        MapPlugin.prototype.getMapBrightness = function () {
            var state = this._getState();
            return state.brightness;
        };
        MapPlugin.prototype.setMapBrightness = function (value) {
            var state = this._getState();
            if (value && !isNaN(value) && state.brightness !== value) {
                if (!state.map) {
                    state.brightness = value;
                }
                else {
                    var container = state.tileLayer.getContainer();
                    container.className = this._setBrightnessClass(container.className, state.brightness, value);
                    state.brightness = value;
                }
                this.saveState();
                this._raiseBrightnessChanged();
            }
        };
        MapPlugin.prototype._setBrightnessClass = function (currentClassName, currentBrightness, newBrightness) {
            var result = currentClassName;
            if (currentBrightness !== newBrightness) {
                if (!result) {
                    result = '';
                }
                var currentBrightnessClass = this._classNameForMapBrightness(currentBrightness);
                if (currentBrightnessClass !== '') {
                    result = result
                        .replace(currentBrightnessClass, '')
                        .trim();
                }
                var newBrightnessClass = this._classNameForMapBrightness(newBrightness);
                if (newBrightnessClass !== '') {
                    if (result.length > 0) {
                        result += ' ';
                    }
                    result += newBrightnessClass;
                }
            }
            return result;
        };
        MapPlugin.prototype._classNameForMapBrightness = function (brightness) {
            var result = '';
            if (brightness && !isNaN(brightness) && brightness > 0 && brightness <= 150 && brightness !== 100 && brightness / 10 === Math.floor(brightness / 10)) {
                result = 'vrs-brightness-' + brightness;
            }
            return result;
        };
        MapPlugin.prototype._targetResized = function () {
            var state = this._getState();
            var center = this._getCenter(state);
            this.refreshMap();
            this._setCenter(state, center);
        };
        return MapPlugin;
    }(JQueryUICustomWidget));
    $.widget('vrs.vrsMap', new MapPlugin());
})(VRS || (VRS = {}));
//# sourceMappingURL=jquery.vrs.map-leaflet.js.map