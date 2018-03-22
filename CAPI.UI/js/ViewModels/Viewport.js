/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>
var VisTarsier;
(function (VisTarsier) {
    var ViewModels;
    (function (ViewModels) {
        var Viewport = /** @class */ (function () {
            function Viewport() {
                var _this = this;
                this.id = -1;
                this.display = function (seriesId, imageNumber) {
                    if (_this.seriesCollection.seriesArray.length < 1)
                        return;
                    var series = _this.seriesCollection.seriesArray.filter(function (s) { return s.id === seriesId; });
                    if (series.length !== 1)
                        return;
                    _this.seriesDisplayedId = seriesId;
                    if (series[0].imagesList.length > imageNumber)
                        _this.imageDisplayedIndex = imageNumber;
                };
                this.addSeries = function (series) {
                    _this.seriesCollection.addSeries(series);
                };
                this.seriesCollection = new ViewModels.SeriesCollection();
            }
            Object.defineProperty(Viewport.prototype, "displayedImagePath", {
                get: function () {
                    if (this.seriesCollection.seriesArray.length === 0)
                        return "";
                    if (this.seriesDisplayed.imagesList.length < this.imageDisplayedIndex + 1)
                        return "";
                    console.log("Viewport: ", this.id, " | ", "seriesDisplayedId: ", this.seriesDisplayedId, " | ", "imageDisplayedIndex: ", this.imageDisplayedIndex);
                    return this.seriesDisplayed.imagesList[this.imageDisplayedIndex];
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(Viewport.prototype, "seriesDisplayed", {
                get: function () {
                    var _this = this;
                    var seriesDisplayed = this.seriesCollection.seriesArray.filter(function (s) {
                        return s.id === _this.seriesDisplayedId;
                    });
                    if (seriesDisplayed.length > 1)
                        throw new Error("Duplicate series Id detected");
                    return seriesDisplayed.length === 1 ? seriesDisplayed[0] : null;
                },
                enumerable: true,
                configurable: true
            });
            Object.defineProperty(Viewport.prototype, "seriesDisplayedIndex", {
                get: function () {
                    var _this = this;
                    if (!this.seriesCollection.seriesArray)
                        return -1;
                    var seriesIndex = -1;
                    this.seriesCollection.seriesArray.forEach(function (s, index) {
                        if (s.id === _this.seriesDisplayedId)
                            seriesIndex = index;
                    });
                    return seriesIndex;
                },
                enumerable: true,
                configurable: true
            });
            Viewport.prototype.contains = function (series) {
                return !this.seriesCollection.seriesArray.every(function (s) { return s.id !== series.id; });
            };
            return Viewport;
        }());
        ViewModels.Viewport = Viewport;
        var ViewportCollection = /** @class */ (function () {
            function ViewportCollection() {
                var _this = this;
                this.areLinked = true;
                this.addViewport = function (viewport) {
                    if (viewport.id < 0)
                        viewport.id = _this.viewportArr.length; // assign 0-based incremental id
                    _this.viewportArr.push(viewport);
                };
                this.displayNextImage = function (viewport) {
                    if (viewport.seriesDisplayed &&
                        viewport.seriesDisplayed.imagesList.length > viewport.imageDisplayedIndex + 1) {
                        var imageIndex_1 = viewport.imageDisplayedIndex;
                        if (_this.areLinked)
                            _this.viewportArr
                                .forEach(function (v) {
                                if (v.seriesDisplayed)
                                    v.display(v.seriesDisplayed.id, imageIndex_1 + 1);
                            });
                        else
                            viewport.display(viewport.seriesDisplayed.id, imageIndex_1 + 1);
                    }
                };
                this.displayPreviousImage = function (viewport) {
                    if (viewport.seriesDisplayed &&
                        viewport.seriesDisplayed.imagesList.length > 0 &&
                        viewport.imageDisplayedIndex > 0) {
                        var imageIndex_2 = viewport.imageDisplayedIndex;
                        if (_this.areLinked)
                            _this.viewportArr
                                .forEach(function (v) {
                                if (v.seriesDisplayed)
                                    v.display(v.seriesDisplayed.id, imageIndex_2 - 1);
                            });
                        else
                            viewport.display(viewport.seriesDisplayed.id, imageIndex_2 - 1);
                    }
                };
                this.displayNextSeries = function (viewport) {
                    console.log("seriesDisplayedIndex:", viewport.seriesDisplayedIndex);
                    var viewportSeriesArr = viewport.seriesCollection.seriesArray;
                    if (viewport.seriesDisplayed &&
                        viewportSeriesArr.length > viewport.seriesDisplayedIndex + 1) {
                        viewport.display(viewportSeriesArr[viewport.seriesDisplayedIndex + 1].id, viewport.imageDisplayedIndex);
                    }
                };
                this.displayPreviousSeries = function (viewport) {
                    console.log("seriesDisplayedIndex:", viewport.seriesDisplayedIndex);
                    var viewportSeriesArr = viewport.seriesCollection.seriesArray;
                    if (viewport.seriesDisplayed &&
                        viewport.seriesDisplayedIndex > 0) {
                        viewport.display(viewportSeriesArr[viewport.seriesDisplayedIndex - 1].id, viewport.imageDisplayedIndex);
                    }
                };
                this.toggleLink = function () {
                    _this.areLinked = !_this.areLinked;
                };
                this.viewportArr = new Array();
            }
            Object.defineProperty(ViewportCollection.prototype, "viewportArray", {
                get: function () {
                    return this.viewportArr;
                },
                enumerable: true,
                configurable: true
            });
            return ViewportCollection;
        }());
        ViewModels.ViewportCollection = ViewportCollection;
    })(ViewModels = VisTarsier.ViewModels || (VisTarsier.ViewModels = {}));
})(VisTarsier || (VisTarsier = {}));
//# sourceMappingURL=D:/Dropbox/Projects/VT/VT-App/D:/Dropbox/Projects/CAPI/Application/CAPI.UI/js/ViewModels/Viewport.js.map