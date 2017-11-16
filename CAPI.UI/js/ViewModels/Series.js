/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>
var VisTarsier;
(function (VisTarsier) {
    var ViewModels;
    (function (ViewModels) {
        var Series = (function () {
            function Series(name, fileFormat, imagesList) {
                var _this = this;
                this.id = -1;
                this.numberOfImages = 0;
                this.viewableFormats = ["bmp", "jpg", "png"];
                this.displayInViewport = function (viewports, viewportIndex, imageIndex) {
                    var series = new Series(_this.name, _this.fileFormat, _this.imagesList);
                    var seriesName = _this.name;
                    if (!_this.isViewable) {
                        var callback = function (response) {
                            var filesArr = response.Data.Data;
                            series = new Series(seriesName, "png", filesArr.map(function (f) { return "img/Viewable/" + f; }));
                            var calllingViewport = viewports.viewportArray[viewportIndex];
                            if (!calllingViewport.contains(series))
                                calllingViewport.addSeries(series);
                            calllingViewport.display(series.id, imageIndex);
                        };
                        _this.convertToViewable("png", callback);
                    }
                };
                this.convertToViewable = function (outFileFormat, callback) {
                    VisTarsier.Shared.Ajax.add("../api/Images/ConvertToViewable", JSON.stringify({ files: _this.imagesList, seriesName: _this.name, outFileFormat: outFileFormat }), callback, null);
                };
                this.name = name;
                this.fileFormat = fileFormat;
                this.imagesList = imagesList;
                this.numberOfImages = imagesList.length;
            }
            Object.defineProperty(Series.prototype, "isViewable", {
                get: function () {
                    return (this.viewableFormats.indexOf(this.fileFormat) > -1);
                },
                enumerable: true,
                configurable: true
            });
            ;
            return Series;
        }());
        ViewModels.Series = Series;
        var SeriesCollection = (function () {
            function SeriesCollection() {
                var _this = this;
                this.addSeries = function (series) {
                    if (series.id < 0)
                        series.id = _this.seriesArr.length; // assign 0-based incremental id
                    _this.seriesArr.push(series);
                };
                this.seriesArr = new Array();
            }
            Object.defineProperty(SeriesCollection.prototype, "seriesArray", {
                get: function () {
                    return this.seriesArr;
                },
                enumerable: true,
                configurable: true
            });
            ;
            return SeriesCollection;
        }());
        ViewModels.SeriesCollection = SeriesCollection;
    })(ViewModels = VisTarsier.ViewModels || (VisTarsier.ViewModels = {}));
})(VisTarsier || (VisTarsier = {}));
//# sourceMappingURL=D:/Dropbox/Projects/VT/VT-App/D:/Dropbox/Projects/CAPI/Application/CAPI.UI/js/ViewModels/Series.js.map