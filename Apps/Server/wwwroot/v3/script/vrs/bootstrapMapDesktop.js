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
    var BootstrapMapDesktop = (function (_super) {
        __extends(BootstrapMapDesktop, _super);
        function BootstrapMapDesktop() {
            return _super.call(this, {
                configPrefix: 'desktop',
                reportUrl: 'desktopReport.html'
            }) || this;
        }
        return BootstrapMapDesktop;
    }(VRS.BootstrapMap));
    VRS.BootstrapMapDesktop = BootstrapMapDesktop;
})(VRS || (VRS = {}));
//# sourceMappingURL=bootstrapMapDesktop.js.map