/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>

namespace VisTarsier.ViewModels {
    export class Viewport {
        id = -1;
        seriesCollection: SeriesCollection;
        imageDisplayedIndex: number;
        seriesDisplayedId: number;

        constructor() {
            this.seriesCollection = new SeriesCollection();
        }

        display = (seriesId: number, imageNumber: number) => {
            if (this.seriesCollection.seriesArray.length < 1) return;
            const series = this.seriesCollection.seriesArray.filter((s) => { return s.id === seriesId; });
            if (series.length !== 1) return;
            this.seriesDisplayedId = seriesId;
            if (series[0].imagesList.length > imageNumber)
                this.imageDisplayedIndex = imageNumber;
        };

        addSeries = (series: Series) => {
            this.seriesCollection.addSeries(series);
        };

        get displayedImagePath() {
            if (this.seriesCollection.seriesArray.length === 0) return "";
            if (this.seriesDisplayed.imagesList.length < this.imageDisplayedIndex + 1) return "";
            console.log("Viewport: ", this.id, " | ", "seriesDisplayedId: ", this.seriesDisplayedId, " | ", "imageDisplayedIndex: ", this.imageDisplayedIndex);
            return this.seriesDisplayed.imagesList[this.imageDisplayedIndex];
        }

        get seriesDisplayed(): Series {
            const seriesDisplayed = this.seriesCollection.seriesArray.filter((s) => {
                return s.id === this.seriesDisplayedId;
            });
            if (seriesDisplayed.length > 1) throw new Error("Duplicate series Id detected");
            return seriesDisplayed.length === 1 ? seriesDisplayed[0] : null;
        }

        get seriesDisplayedIndex(): number {
            if (!this.seriesCollection.seriesArray) return -1;
            var seriesIndex = -1;
            this.seriesCollection.seriesArray.forEach((s, index) => {
                if (s.id === this.seriesDisplayedId) seriesIndex = index;
            });
            return seriesIndex;
        }

        contains(series: Series): boolean {
            return !this.seriesCollection.seriesArray.every((s) => { return s.id !== series.id; });
        }
    }

    export class ViewportCollection {
        private viewportArr: Array<Viewport>;
        get viewportArray(): Array<Viewport> {
            return this.viewportArr;
        }
        areLinked = true;

        constructor() {
            this.viewportArr = new Array<Viewport>();
        }

        addViewport = (viewport: Viewport) => {
            if (viewport.id < 0)
                viewport.id = this.viewportArr.length; // assign 0-based incremental id
            this.viewportArr.push(viewport);
        };

        displayNextImage = (viewport: Viewport) => {
            if (viewport.seriesDisplayed &&
                viewport.seriesDisplayed.imagesList.length > viewport.imageDisplayedIndex + 1) {
                const imageIndex = viewport.imageDisplayedIndex;
                if (this.areLinked) this.viewportArr
                    .forEach((v) => {
                        if (v.seriesDisplayed) v.display(v.seriesDisplayed.id, imageIndex + 1);
                    });
                else viewport.display(viewport.seriesDisplayed.id, imageIndex + 1);
            }
        }

        displayPreviousImage = (viewport: Viewport) => {
            if (viewport.seriesDisplayed &&
                viewport.seriesDisplayed.imagesList.length > 0 &&
                viewport.imageDisplayedIndex > 0) {
                const imageIndex = viewport.imageDisplayedIndex;
                if (this.areLinked) this.viewportArr
                    .forEach((v) => {
                        if (v.seriesDisplayed) v.display(v.seriesDisplayed.id, imageIndex - 1);
                    });
                else viewport.display(viewport.seriesDisplayed.id, imageIndex - 1);
            }
        }

        displayNextSeries = (viewport: Viewport) => {
            console.log("seriesDisplayedIndex:",viewport.seriesDisplayedIndex);
            var viewportSeriesArr = viewport.seriesCollection.seriesArray;
            if (viewport.seriesDisplayed &&
                viewportSeriesArr.length > viewport.seriesDisplayedIndex + 1) {
                viewport.display(viewportSeriesArr[viewport.seriesDisplayedIndex + 1].id,
                    viewport.imageDisplayedIndex);
            }
        }

        displayPreviousSeries = (viewport: Viewport) => {
            console.log("seriesDisplayedIndex:", viewport.seriesDisplayedIndex);
            var viewportSeriesArr = viewport.seriesCollection.seriesArray;
            if (viewport.seriesDisplayed &&
                viewport.seriesDisplayedIndex > 0) {
                viewport.display(viewportSeriesArr[viewport.seriesDisplayedIndex - 1].id,
                    viewport.imageDisplayedIndex);
            }
        }

        toggleLink = () => {
            this.areLinked = !this.areLinked;
        }
    }
}