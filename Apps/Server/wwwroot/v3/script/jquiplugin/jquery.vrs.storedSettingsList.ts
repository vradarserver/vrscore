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
 * @fileoverview A jQuery UI widget that displays all of the configuration settings held in persistent storage and allows the user to manipulate them.
 */

namespace VRS
{
    /**
     * The state object for the StoredSettingsPlugin.
     */
    class StoredSettingsList_State
    {
        /**
         * The container that lists all of the VRS keys.
         */
        keysContainer: JQuery = null;

        /**
         * The container that holds the content of a single key.
         */
        keyContentContainer: JQuery = null;

        /**
         * The currently selected key.
         */
        currentKey: string = null;

        /**
         * The textarea element that shows the import/export settings.
         */
        importExportElement: JQuery = null;

        /**
         * The container that holds all of the import controls.
         */
        importControlsContainer: JQuery = null;
    }

    /**
     * A jQuery UI widget that can show the user all of the settings stored at the browser and let them manipulate them.
     */
    export class StoredSettingsList extends JQueryUICustomWidget
    {
        options: {}

        private _getState() : StoredSettingsList_State
        {
            var result = this.element.data('storedSettingsState');
            if(result === undefined) {
                result = new StoredSettingsList_State();
                this.element.data('storedSettingsState', result);
            }

            return result;
        }

        _create()
        {
            var state = this._getState();

            this.element.addClass('vrsStoredSettings');

            var buttonBlock =
                $('<div/>')
                    .appendTo(this.element);
            $('<button />')
                .text(VRS.$$.RemoveAll)
                .click(() => this._removeAllKeys())
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.Refresh)
                .click(() => this._refreshDisplay())
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.ExportSettings)
                .click(() => this._exportSettings())
                .appendTo(buttonBlock);
            $('<button />')
                .text(VRS.$$.ImportSettings)
                .click(() => this._showImportControls())
                .appendTo(buttonBlock);

            var importExport =
                $('<div />')
                    .attr('class', 'importExport')
                    .appendTo(this.element);

            state.importExportElement = $('<textarea />')
                .hide()
                .appendTo(importExport);

            state.importControlsContainer =
                $('<div />')
                    .hide()
                    .attr('class', 'import')
                    .appendTo(importExport);

            var checkboxesContainer =
                $('<ol />')
                    .appendTo(state.importControlsContainer);

            var importOverwrite =               this._addCheckBox(checkboxesContainer, VRS.$$.OverwriteExistingSettings, true);
            var importReset =                   this._addCheckBox(checkboxesContainer, VRS.$$.EraseBeforeImport, true);
            var importIgnoreRequestFeedId =     this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportRequestFeedId, true);
            var importIgnoreLanguage =          this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportLanguageSettings, false);
            var importIgnoreSplitters =         this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportSplitters, false);
            var importIgnoreCurrentLocation =   this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportCurrentLocation, false);
            var importIgnoreAutoSelect =        this._addCheckBox(checkboxesContainer, VRS.$$.DoNotImportAutoSelect, false);

            var importButton =
                $('<button />')
                    .text(VRS.$$.Import)
                    .click(() =>
                        this._importSettings({
                            overwrite:              importOverwrite.prop('checked'),
                            resetBeforeImport:      importReset.prop('checked'),
                            ignoreLanguage:         importIgnoreLanguage.prop('checked'),
                            ignoreSplitters:        importIgnoreSplitters.prop('checked'),
                            ignoreCurrentLocation:  importIgnoreCurrentLocation.prop('checked'),
                            ignoreAutoSelect:       importIgnoreAutoSelect.prop('checked'),
                            ignoreRequestFeedId:    importIgnoreRequestFeedId.prop('checked')
                        })
                    )
                    .appendTo(state.importControlsContainer);

            state.keysContainer =
                $('<div/>')
                    .addClass('keys')
                    .appendTo(this.element);
            state.keyContentContainer =
                $('<div/>')
                    .addClass('content')
                    .appendTo(this.element);

            this._buildKeysTable(state);
        }

        /**
         * Appends a checkbox to the control.
         */
        private _addCheckBox(parentElement: JQuery, labelText: string, initialCheckedState: boolean) : JQuery
        {
            var listItem = $('<li />')
                .appendTo(parentElement);

            var result = $('<input />')
                .uniqueId()
                .attr('type', 'checkbox')
                .prop('checked', initialCheckedState)
                .appendTo(listItem);

            $('<label />')
                .attr('for', result.attr('id'))
                .text(labelText)
                .appendTo(listItem);

            return result;
        }

        /**
         * Builds a table to display the keys that are in use.
         */
        private _buildKeysTable(state: StoredSettingsList_State)
        {
            state.keysContainer.empty();

            var statistics =
                $('<table/>')
                    .addClass('statistics')
                    .appendTo(state.keysContainer)
                    .append(
                        $('<tr/>')
                            .append($('<td/>').text(VRS.$$.StorageEngine + ':'))
                            .append($('<td/>').text(VRS.configStorage.getStorageEngine()))
                    )
                    .append(
                        $('<tr/>')
                            .append($('<td/>').text(VRS.$$.StorageSize + ':'))
                            .append($('<td/>').text(VRS.configStorage.getStorageSize()))
                    );
            var list = $('<ul/>')
                .appendTo(state.keysContainer);

            var hasContent = false;
            var keys = VRS.configStorage.getAllVirtualRadarKeys().sort();
            $.each(keys, (idx, key) => {
                hasContent = true;
                var keyName = String(key);
                var listItem =
                        $('<li/>')
                            .text(keyName)
                            .click(() => this._keyClicked(keyName))
                            .appendTo(list);
                if(keyName === state.currentKey) {
                    listItem.addClass('current');
                }
            });
            if(!hasContent) {
                list.append(
                    $('<li/>')
                        .text(VRS.$$.NoSettingsFound)
                        .addClass('empty')
                );
            }
        }

        /**
         * Displays a single key's content to the user.
         */
        private _showKeyContent(keyName: string, content: Object)
        {
            var state = this._getState();
            var container = state.keyContentContainer;
            var self = this;

            state.currentKey = keyName;
            container.empty();

            if(keyName) {
                $('<p/>')
                    .addClass('keyTitle')
                    .text(keyName)
                    .appendTo(container);

                var contentDump = $('<code/>').appendTo(container);
                if(content === null || content === undefined) {
                    contentDump
                        .addClass('empty')
                        .text(content === null ? '<null>' : '<undefined>');
                } else {
                    var json = $.toJSON(content);// what are these parameters? ->  , null, 4);
                    json = json
                        .replace(/&/g, '&amp;')
                        .replace(/ /g, '&nbsp')
                        .replace(/</g, '&lt;')
                        .replace(/>/g, '&gt;')
                        .replace(/\t/g, '&nbsp;&nbsp;&nbsp;&nbsp;')
                        .replace(/\n/g, '<br />');
                    contentDump.html(json);
                }

                var buttonBlock = $('<div/>')
                    .addClass('buttonBlock')
                    .appendTo(container);
                $('<button/>')
                    .text(VRS.$$.Remove)
                    .click(() => this._removeKey(keyName))
                    .appendTo(buttonBlock);
            }

            this._buildKeysTable(state);
        }

        /**
         * Refreshes the display of keys and content.
         */
        private _refreshDisplay()
        {
            var state = this._getState();

            var content = null;
            if(state.currentKey) {
                var exists = false;
                $.each(VRS.configStorage.getAllVirtualRadarKeys(), function() {
                    exists = String(this) === state.currentKey;
                    return !exists;
                });
                if(!exists) {
                    state.currentKey = null;
                } else {
                    content = VRS.configStorage.getContentWithoutPrefix(state.currentKey);
                }
            }
            this._showKeyContent(state.currentKey, content);
        }

        /**
         * Removes the configuration settings associated with the key passed across.
         */
        private _removeKey(keyName: string)
        {
            var state = this._getState();
            VRS.configStorage.removeContentWithoutPrefix(keyName);
            state.currentKey = null;

            this._buildKeysTable(state);
            state.keyContentContainer.empty();
        }

        /**
         * Removes all configuration settings.
         */
        private _removeAllKeys()
        {
            var state = this._getState();
            state.currentKey = null;
            VRS.configStorage.removeAllContent();
            state.keyContentContainer.empty();
            this._buildKeysTable(state);
        }

        /**
         * Creates and displays the serialised settings.
         */
        private _exportSettings()
        {
            var state = this._getState();
            state.importControlsContainer.hide();

            var element = state.importExportElement;
            if(element.is(':visible')) {
                element.hide();
            } else {
                element.val('');
                element.show();

                var settings = VRS.configStorage.exportSettings();
                element.val(settings);
            }
        }

        /**
         * Displays the import controls.
         */
        private _showImportControls()
        {
            var state = this._getState();

            var element = state.importExportElement;
            if(!element.is(':visible')) {
                element.show();
                element.val('');
                state.importControlsContainer.show();
            } else {
                element.hide();
                state.importControlsContainer.hide();
            }
        }

        /**
         * Takes the text in the import textarea and attempts to import it.
         */
        private _importSettings(options: ConfigStorage_ImportOptions)
        {
            var state = this._getState();

            var text = state.importExportElement.val();
            if(text) {
                state.currentKey = null;
                try {
                    VRS.configStorage.importSettings(text, options);
                    this._refreshDisplay();
                } catch(ex) {
                    VRS.pageHelper.showMessageBox(VRS.$$.ImportFailedTitle, VRS.stringUtility.format(VRS.$$.ImportFailedBody, ex));
                }
            }
        }

        /**
         * Called when the user clicks on an entry for the key in the keys table.
         * @param {string} keyName  The name of the key that was clicked.
         * @private
         */
        private _keyClicked(keyName: string)
        {
            var content = VRS.configStorage.getContentWithoutPrefix(keyName);
            this._showKeyContent(keyName, content);
        }
    }

    $.widget('vrs.vrsStoredSettingsList', new StoredSettingsList());
}

declare interface JQuery
{
    vrsStoredSettingsList();
    vrsStoredSettingsList(options: {});
    vrsStoredSettingsList(methodName: string, param1?: any, param2?: any, param3?: any, param4?: any);
}
