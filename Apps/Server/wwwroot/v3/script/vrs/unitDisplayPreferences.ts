﻿/**
 * @license Copyright © 2013 onwards, Andrew Whewell
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
 * @fileoverview Code to record and configure the units of measurement shown for various heights, distances etc.
 */

namespace VRS
{
    /*
     * Global options
     */
    export var globalOptions: GlobalOptions = VRS.globalOptions || {};
    export var isFlightSim = VRS.isFlightSim || false;      // <-- true if the flight sim page was loaded
    VRS.globalOptions.unitDisplayHeight = VRS.globalOptions.unitDisplayHeight || VRS.Height.Feet;                                   // The default unit for altitudes.
    VRS.globalOptions.unitDisplaySpeed = VRS.globalOptions.unitDisplaySpeed || VRS.Speed.Knots;                                     // The default unit for speeds.
    VRS.globalOptions.unitDisplayDistance = VRS.globalOptions.unitDisplayDistance || VRS.Distance.Kilometre;                        // The default unit for distances.
    VRS.globalOptions.unitDisplayPressure = VRS.globalOptions.unitDisplayPressure || VRS.Pressure.InHg;                             // The default unit for pressures.
    VRS.globalOptions.unitDisplayVsiPerSecond = VRS.globalOptions.unitDisplayVsiPerSecond !== undefined ? VRS.globalOptions.unitDisplayVsiPerSecond : VRS.isFlightSim;          // True if vertical speeds are to be shown per second rather than per minute.
    VRS.globalOptions.unitDisplayFLTransitionAltitude = VRS.globalOptions.unitDisplayFLTransitionAltitude || 18000;                 // The default flight level transition altitude.
    VRS.globalOptions.unitDisplayFLTransitionHeightUnit = VRS.globalOptions.unitDisplayFLTransitionHeightUnit || VRS.Height.Feet;   // The units that FLTransitionAltitude is in.
    VRS.globalOptions.unitDisplayFLHeightUnit = VRS.globalOptions.unitDisplayFLHeightUnit || VRS.Height.Feet;                       // The units that flight levels are displayed in.
    VRS.globalOptions.unitDisplayAllowConfiguration = VRS.globalOptions.unitDisplayAllowConfiguration !== undefined ? VRS.globalOptions.unitDisplayAllowConfiguration : true;   // True if users can configure the display units, false if they cannot.
    VRS.globalOptions.unitDisplayAltitudeType = VRS.globalOptions.unitDisplayAltitudeType !== undefined ? VRS.globalOptions.unitDisplayAltitudeType : false;                // True if altitude types are to be shown by default.
    VRS.globalOptions.unitDisplayVerticalSpeedType = VRS.globalOptions.unitDisplayVerticalSpeedType !== undefined ? VRS.globalOptions.unitDisplayVerticalSpeedType : false; // True if vertical speed types are to be shown by default.
    VRS.globalOptions.unitDisplaySpeedType = VRS.globalOptions.unitDisplaySpeedType !== undefined ? VRS.globalOptions.unitDisplaySpeedType : true;                          // True if speed types are to be shown by default.
    VRS.globalOptions.unitDisplayTrackType = VRS.globalOptions.unitDisplayTrackType !== undefined ? VRS.globalOptions.unitDisplayTrackType : false;                         // True if track types are to be shown by default.
    VRS.globalOptions.unitDisplayUsePressureAltitude = VRS.globalOptions.unitDisplayUsePressureAltitude !== undefined ? VRS.globalOptions.unitDisplayUsePressureAltitude : true;    // True to use pressure altitudes when we have a choice between pressure and geometric altitudes.

    /**
     * The state object that instances of UnitDisplayPreferences save between sessions.
     */
    export interface UnitDisplayPreferences_SaveState
    {
        distanceUnit:           DistanceEnum;
        heightUnit:             HeightEnum;
        speedUnit:              SpeedEnum;
        pressureUnit:           PressureEnum;
        vsiPerSecond:           boolean;
        flTransitionAlt:        number;
        flTransitionUnit:       HeightEnum;
        flHeightUnit:           HeightEnum;
        showAltType:            boolean;
        showVsiType:            boolean;
        showSpeedType:          boolean;
        showTrackType:          boolean;
        usePressureAltitude:    boolean;
    }

    /**
     * A class that brings together the various units of measurement that the user can configure.
     */
    export class UnitDisplayPreferences implements ISelfPersist<UnitDisplayPreferences_SaveState>
    {
        private _Dispatcher = new VRS.EventHandler({
            name: 'VRS.UnitDisplayPreferences'
        });
        private _Events = {
            unitChanged:                    'unitChanged',
            distanceUnitChanged:            'distanceUnitChanged',
            heightUnitChanged:              'heightUnitChanged',
            speedUnitChanged:               'speedUnitChanged',
            pressureUnitChanged:            'pressureUnitChanged',
            showVsiInSecondsChanged:        'showVsiSecondsChanged',
            flAltitudeChanged:              'flAltitudeChanged',
            flTransUnitChanged:             'flTransUnitChanged',
            flHeightUnitChanged:            'flHeightUnitChanged',
            showAltitudeTypeChanged:        'showAltTypeChanged',
            showVerticalSpeedTypeChanged:   'showVsiTypeChanged',
            showSpeedTypeChanged:           'showSpeedTypeChanged',
            showTrackTypeChanged:           'showTrackTypeChanged',
            usePressureAltitudeChanged:     'usePressureAltitudeChanged'
        }

        private _Name: string;
        private _DistanceUnit: DistanceEnum;
        private _HeightUnit: HeightEnum;
        private _SpeedUnit: SpeedEnum;
        private _PressureUnit: PressureEnum;
        private _ShowVerticalSpeedPerSecond: boolean;
        private _ShowAltitudeType: boolean;
        private _ShowVerticalSpeedType: boolean;
        private _ShowSpeedType: boolean;
        private _ShowTrackType: boolean;
        private _UsePressureAltitude: boolean;
        private _FlightLevelTransitionAltitude: number;
        private _FlightLevelTransitionHeightUnit: HeightEnum;
        private _FlightLevelHeightUnit: HeightEnum;

        constructor(name?: string)
        {
            this._Name = name || 'vrsUnitDisplayPreferences';
            this._DistanceUnit = VRS.globalOptions.unitDisplayDistance;
            this._HeightUnit = VRS.globalOptions.unitDisplayHeight;
            this._SpeedUnit = VRS.globalOptions.unitDisplaySpeed;
            this._PressureUnit = VRS.globalOptions.unitDisplayPressure;
            this._ShowVerticalSpeedPerSecond = VRS.globalOptions.unitDisplayVsiPerSecond;
            this._ShowAltitudeType = VRS.globalOptions.unitDisplayAltitudeType;
            this._ShowVerticalSpeedType = VRS.globalOptions.unitDisplayVerticalSpeedType;
            this._ShowSpeedType = VRS.globalOptions.unitDisplaySpeedType;
            this._ShowTrackType = VRS.globalOptions.unitDisplayTrackType;
            this._UsePressureAltitude = VRS.globalOptions.unitDisplayUsePressureAltitude;
            this._FlightLevelTransitionAltitude = VRS.globalOptions.unitDisplayFLTransitionAltitude;
            this._FlightLevelTransitionHeightUnit = VRS.globalOptions.unitDisplayFLTransitionHeightUnit;
            this._FlightLevelHeightUnit = VRS.globalOptions.unitDisplayFLHeightUnit;

            if(VRS.serverConfig) {
                var config = VRS.serverConfig.get();
                if(config) {
                    this.setDistanceUnit(config.InitialDistanceUnit);
                    this.setHeightUnit(config.InitialHeightUnit);
                    this.setSpeedUnit(config.InitialSpeedUnit);
                }
            }
        }

        getName = () : string =>
        {
            return this._Name;
        }

        getDistanceUnit = () : DistanceEnum =>
        {
            return this._DistanceUnit;
        }
        setDistanceUnit = (value: DistanceEnum) =>
        {
            if(this._DistanceUnit !== value) {
                this._DistanceUnit = value;
                this._Dispatcher.raise(this._Events.distanceUnitChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Distance ]);
            }
        }

        getHeightUnit = () : HeightEnum =>
        {
            return this._HeightUnit;
        }
        setHeightUnit = (value: HeightEnum) =>
        {
            if(this._HeightUnit !== value) {
                this._HeightUnit = value;
                this._Dispatcher.raise(this._Events.heightUnitChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Height ]);
            }
        }

        getSpeedUnit = () : SpeedEnum =>
        {
            return this._SpeedUnit;
        }
        setSpeedUnit = (value: SpeedEnum) =>
        {
            if(this._SpeedUnit !== value) {
                this._SpeedUnit = value;
                this._Dispatcher.raise(this._Events.speedUnitChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Speed ]);
            }
        }

        getPressureUnit = () : PressureEnum =>
        {
            return this._PressureUnit;
        }
        setPressureUnit = (value: PressureEnum) =>
        {
            if(this._PressureUnit !== value) {
                this._PressureUnit = value;
                this._Dispatcher.raise(this._Events.pressureUnitChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Pressure ]);
            }
        }

        getShowVerticalSpeedPerSecond = () : boolean =>
        {
            return this._ShowVerticalSpeedPerSecond;
        }
        setShowVerticalSpeedPerSecond = (value: boolean) =>
        {
            if(this._ShowVerticalSpeedPerSecond !== value) {
                this._ShowVerticalSpeedPerSecond = value;
                this._Dispatcher.raise(this._Events.showVsiInSecondsChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.VsiSeconds ]);
            }
        }

        getShowAltitudeType = () : boolean =>
        {
            return this._ShowAltitudeType;
        }
        setShowAltitudeType = (value: boolean) =>
        {
            if(this._ShowAltitudeType !== value) {
                this._ShowAltitudeType = value;
                this._Dispatcher.raise(this._Events.showAltitudeTypeChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Height ]);
            }
        }

        getShowVerticalSpeedType = () : boolean =>
        {
            return this._ShowVerticalSpeedType;
        }
        setShowVerticalSpeedType = (value: boolean) =>
        {
            if(this._ShowVerticalSpeedType !== value) {
                this._ShowVerticalSpeedType = value;
                this._Dispatcher.raise(this._Events.showVerticalSpeedTypeChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Height ]);
            }
        }

        getShowSpeedType = () : boolean =>
        {
            return this._ShowSpeedType;
        }
        setShowSpeedType = (value: boolean) =>
        {
            if(this._ShowSpeedType !== value) {
                this._ShowSpeedType = value;
                this._Dispatcher.raise(this._Events.showSpeedTypeChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Speed ]);
            }
        }

        getShowTrackType = () : boolean =>
        {
            return this._ShowTrackType;
        }
        setShowTrackType = (value: boolean) =>
        {
            if(this._ShowTrackType !== value) {
                this._ShowTrackType = value;
                this._Dispatcher.raise(this._Events.showTrackTypeChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Angle ]);
            }
        }

        getUsePressureAltitude = () : boolean =>
        {
            return this._UsePressureAltitude;
        }
        setUsePressureAltitude = (value: boolean) =>
        {
            if(this._UsePressureAltitude !== value) {
                this._UsePressureAltitude = value;
                this._Dispatcher.raise(this._Events.usePressureAltitudeChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.Height ]);
            }
        }

        getFlightLevelTransitionAltitude = () : number =>
        {
            return this._FlightLevelTransitionAltitude;
        }
        setFlightLevelTransitionAltitude = (value: number) =>
        {
            if(this._FlightLevelTransitionAltitude !== value) {
                this._FlightLevelTransitionAltitude = value;
                this._Dispatcher.raise(this._Events.flAltitudeChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.FLTransitionAltitude ]);
            }
        }

        getFlightLevelTransitionHeightUnit = () : HeightEnum =>
        {
            return this._FlightLevelTransitionHeightUnit;
        }
        setFlightLevelTransitionHeightUnit = (value: HeightEnum) =>
        {
            if(this._FlightLevelTransitionHeightUnit !== value) {
                this._FlightLevelTransitionHeightUnit = value;
                this._Dispatcher.raise(this._Events.flTransUnitChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.FLTransitionHeightUnit ]);
            }
        }

        getFlightLevelHeightUnit = () : HeightEnum =>
        {
            return this._FlightLevelHeightUnit;
        }
        setFlightLevelHeightUnit = (value: HeightEnum) =>
        {
            if(this._FlightLevelHeightUnit !== value) {
                this._FlightLevelHeightUnit = value;
                this._Dispatcher.raise(this._Events.flHeightUnitChanged);
                this._Dispatcher.raise(this._Events.unitChanged, [ VRS.DisplayUnitDependency.FLHeightUnit ]);
            }
        }

        /**
         * Returns an array of every possible altitude unit value and their text keys.
         */
        static getAltitudeUnitValues() : ValueText[]
        {
            return [
                new VRS.ValueText({ value: VRS.Height.Feet,   textKey: 'Feet' }),
                new VRS.ValueText({ value: VRS.Height.Metre,  textKey: 'Metres' })
            ];
        }

        /**
         * Returns an array of every possible distance unit value and their text keys.
         */
        static getDistanceUnitValues() : ValueText[]
        {
            return [
                new VRS.ValueText({ value: VRS.Distance.Kilometre,    textKey: 'Kilometres' }),
                new VRS.ValueText({ value: VRS.Distance.NauticalMile, textKey: 'NauticalMiles' }),
                new VRS.ValueText({ value: VRS.Distance.StatuteMile,  textKey: 'StatuteMiles' })
            ];
        }

        /**
         * Returns an array of every possible speed unit value and their text keys.
         */
        static getSpeedUnitValues() : ValueText[]
        {
            return [
                new VRS.ValueText({ value: VRS.Speed.KilometresPerHour,   textKey: 'KilometresPerHour' }),
                new VRS.ValueText({ value: VRS.Speed.Knots,               textKey: 'Knots' }),
                new VRS.ValueText({ value: VRS.Speed.MilesPerHour,        textKey: 'MilesPerHour' })
            ];
        }

        /**
         * Returns an array of every possible pressure unit value and their text keys.
         */
        static getPressureUnitValues() : ValueText[]
        {
            return [
                new VRS.ValueText({ value: VRS.Pressure.InHg,       textKey: 'InHgDescription' }),
                new VRS.ValueText({ value: VRS.Pressure.Millibar,   textKey: 'MillibarDescription' }),
                new VRS.ValueText({ value: VRS.Pressure.MmHg,       textKey: 'MmHgDescription' })
            ];
        }

        hookDistanceUnitChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.distanceUnitChanged, callback, forceThis);
        }

        hookHeightUnitChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.heightUnitChanged, callback, forceThis);
        }

        hookSpeedUnitChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.speedUnitChanged, callback, forceThis);
        }

        hookPressureUnitChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.pressureUnitChanged, callback, forceThis);
        }

        hookShowVerticalSpeedPerSecondChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.showVsiInSecondsChanged, callback, forceThis);
        }

        hookShowAltitudeTypeChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.showAltitudeTypeChanged, callback, forceThis);
        }

        hookShowVerticalSpeedTypeChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.showVerticalSpeedTypeChanged, callback, forceThis);
        }

        hookShowSpeedTypeChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.showSpeedTypeChanged, callback, forceThis);
        }

        hookShowTrackTypeChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.showTrackTypeChanged, callback, forceThis);
        }

        hookFlightLevelTransitionAltitudeChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.flAltitudeChanged, callback, forceThis);
        }

        hookFlightLevelTransitionHeightUnitChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.flTransUnitChanged, callback, forceThis);
        }

        hookFlightLevelHeightUnitChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.flHeightUnitChanged, callback, forceThis);
        }

        hookUsePressureAltitudeChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.usePressureAltitudeChanged, callback, forceThis);
        }

        /**
         * Raised when any of the units have been changed.
         */
        hookUnitChanged = (callback: (dependency?: DisplayUnitDependencyEnum) => void, forceThis?: Object) : IEventHandle =>
        {
            return this._Dispatcher.hook(this._Events.unitChanged, callback, forceThis);
        }

        unhook = (hookResult: IEventHandle) =>
        {
            this._Dispatcher.unhook(hookResult);
        }

        /**
         * Creates the option pane for the configuration of display units.
         */
        createOptionPane = (displayOrder: number) : OptionPane =>
        {
            var self = this;
            var pane = new VRS.OptionPane({
                name:           'vrsUnitDisplayPreferences_' + this._Name,
                titleKey:       'PaneUnits',
                displayOrder:   displayOrder
            });

            var distanceUnitValues = UnitDisplayPreferences.getDistanceUnitValues();
            var altitudeUnitValues = UnitDisplayPreferences.getAltitudeUnitValues();
            var speedUnitValues = UnitDisplayPreferences.getSpeedUnitValues();
            var pressureUnitValues = UnitDisplayPreferences.getPressureUnitValues();

            if(VRS.globalOptions.unitDisplayAllowConfiguration) {
                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showVsiInSeconds',
                    labelKey:       'ShowVsiInSeconds',
                    getValue:       this.getShowVerticalSpeedPerSecond,
                    setValue:       this.setShowVerticalSpeedPerSecond,
                    saveState:      this.saveState
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showAltitudeType',
                    labelKey:       'ShowAltitudeType',
                    getValue:       this.getShowAltitudeType,
                    setValue:       this.setShowAltitudeType,
                    saveState:      this.saveState
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showVerticalSpeedType',
                    labelKey:       'ShowVerticalSpeedType',
                    getValue:       this.getShowVerticalSpeedType,
                    setValue:       this.setShowVerticalSpeedType,
                    saveState:      this.saveState
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showSpeedType',
                    labelKey:       'ShowSpeedType',
                    getValue:       this.getShowSpeedType,
                    setValue:       this.setShowSpeedType,
                    saveState:      this.saveState
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'showTrackType',
                    labelKey:       'ShowTrackType',
                    getValue:       this.getShowTrackType,
                    setValue:       this.setShowTrackType,
                    saveState:      this.saveState
                }));

                pane.addField(new VRS.OptionFieldCheckBox({
                    name:           'usePressureAltitude',
                    labelKey:       'UsePressureAltitude',
                    getValue:       this.getUsePressureAltitude,
                    setValue:       this.setUsePressureAltitude,
                    saveState:      this.saveState
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'distanceUnit',
                    labelKey:       'Distances',
                    getValue:       this.getDistanceUnit,
                    setValue:       this.setDistanceUnit,
                    saveState:      this.saveState,
                    values:         distanceUnitValues
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'heightUnit',
                    labelKey:       'Heights',
                    getValue:       this.getHeightUnit,
                    setValue:       this.setHeightUnit,
                    saveState:      this.saveState,
                    values:         altitudeUnitValues
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'speedUnit',
                    labelKey:       'Speeds',
                    getValue:       this.getSpeedUnit,
                    setValue:       this.setSpeedUnit,
                    saveState:      this.saveState,
                    values:         speedUnitValues
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'pressureUnit',
                    labelKey:       'Pressures',
                    getValue:       this.getPressureUnit,
                    setValue:       this.setPressureUnit,
                    saveState:      this.saveState,
                    values:         pressureUnitValues
                }));

                pane.addField(new VRS.OptionFieldNumeric({
                    name:           'flTransAltitude',
                    labelKey:       'FlightLevelTransitionAltitude',
                    getValue:       this.getFlightLevelTransitionAltitude,
                    setValue:       this.setFlightLevelTransitionAltitude,
                    saveState:      this.saveState,
                    inputWidth:     VRS.InputWidth.SixChar,
                    min:            -10000,
                    max:            99900,
                    decimals:       0,
                    step:           100,
                    keepWithNext:   true
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'flTransUnit',
                    getValue:       this.getFlightLevelTransitionHeightUnit,
                    setValue:       this.setFlightLevelTransitionHeightUnit,
                    saveState:      this.saveState,
                    values:         altitudeUnitValues
                }));

                pane.addField(new VRS.OptionFieldComboBox({
                    name:           'flHeightUnit',
                    labelKey:       'FlightLevelHeightUnit',
                    getValue:       this.getFlightLevelHeightUnit,
                    setValue:       this.setFlightLevelHeightUnit,
                    saveState:      this.saveState,
                    values:         altitudeUnitValues
                }));
            }

            return pane;
        }

        /**
         * Stores the current state of the object.
         */
        saveState = () =>
        {
            VRS.configStorage.saveWithoutPrefix(this.persistenceKey(), this.createSettings());
        }

        /**
         * Returns the previously stored state of the object or the current state if no state was previously saved.
         */
        loadState = () : UnitDisplayPreferences_SaveState =>
        {
            var savedSettings = VRS.configStorage.loadWithoutPrefix(this.persistenceKey(), {});
            return $.extend(this.createSettings(), savedSettings);
        }

        /**
         * Applies a previously stored state to the object.
         */
        applyState = (settings: UnitDisplayPreferences_SaveState) =>
        {
            this.setDistanceUnit(settings.distanceUnit);
            this.setHeightUnit(settings.heightUnit);
            this.setSpeedUnit(settings.speedUnit);
            this.setPressureUnit(settings.pressureUnit);
            this.setShowVerticalSpeedPerSecond(settings.vsiPerSecond);
            this.setFlightLevelTransitionAltitude(settings.flTransitionAlt);
            this.setFlightLevelTransitionHeightUnit(settings.flTransitionUnit);
            this.setFlightLevelHeightUnit(settings.flHeightUnit);
            this.setShowAltitudeType(settings.showAltType);
            this.setShowVerticalSpeedType(settings.showVsiType);
            this.setShowSpeedType(settings.showSpeedType);
            this.setShowTrackType(settings.showTrackType);
            this.setUsePressureAltitude(settings.usePressureAltitude);
        }

        /**
         * Loads and applies a previously stored state.
         */
        loadAndApplyState = () =>
        {
            this.applyState(this.loadState());
        }

        /**
         * Returns the key used to store the state.
         */
        private persistenceKey = () : string =>
        {
            return 'unitDisplayPreferences-' + this.getName();
        }

        /**
         * Returns the current state of the object.
         */
        private createSettings = () : UnitDisplayPreferences_SaveState =>
        {
            return {
                distanceUnit:           this.getDistanceUnit(),
                heightUnit:             this.getHeightUnit(),
                speedUnit:              this.getSpeedUnit(),
                pressureUnit:           this.getPressureUnit(),
                vsiPerSecond:           this.getShowVerticalSpeedPerSecond(),
                flTransitionAlt:        this.getFlightLevelTransitionAltitude(),
                flTransitionUnit:       this.getFlightLevelTransitionHeightUnit(),
                flHeightUnit:           this.getFlightLevelHeightUnit(),
                showAltType:            this.getShowAltitudeType(),
                showVsiType:            this.getShowVerticalSpeedType(),
                showSpeedType:          this.getShowSpeedType(),
                showTrackType:          this.getShowTrackType(),
                usePressureAltitude:    this.getUsePressureAltitude()
            };
        }
    }
} 