/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>

namespace VisTarsier.ViewModels {
    export class Series {
        id = -1;
        name: string;
        fileFormat: string;
        imagesList: string[];
        numberOfImages = 0;
        viewableFormats = ["bmp","jpg","png"];
        get isViewable(): boolean {
            return (this.viewableFormats.indexOf(this.fileFormat) > -1);
        };

        constructor(name: string, fileFormat: string, imagesList: string[]) {
            this.name = name;
            this.fileFormat = fileFormat;
            this.imagesList = imagesList;
            this.numberOfImages = imagesList.length;
        }

        displayInViewport = (viewports: ViewportCollection, viewportIndex: number, imageIndex: number) => {
            var series = new Series(this.name, this.fileFormat, this.imagesList);
            const seriesName = this.name;
            if (!this.isViewable) {
                const callback = (response: any) => {
                    const filesArr = response.Data.Data as string[];
                    series = new Series(seriesName, "png", filesArr.map((f) => { return `img/Viewable/${f}`; }));
                    const calllingViewport = viewports.viewportArray[viewportIndex];
                    if (!calllingViewport.contains(series)) calllingViewport.addSeries(series);
                    calllingViewport.display(series.id, imageIndex);
                };
                this.convertToViewable("png", callback);
            } 
        };

        convertToViewable = (outFileFormat: string, callback) => {
            Shared.Ajax.add("../api/Images/ConvertToViewable"
                , JSON.stringify({ files: this.imagesList, seriesName: this.name, outFileFormat: outFileFormat })
                , callback, null
            );
        };
    }

    export class SeriesCollection {
        private seriesArr: Array<Series>;
        get seriesArray(): Array<Series> {
            return this.seriesArr;
        };

        addSeries = (series: Series) => {
            if (series.id < 0)
                series.id = this.seriesArr.length;  // assign 0-based incremental id
            this.seriesArr.push(series);
        };
        
        constructor() {
            this.seriesArr = new Array<Series>();
        }
    }
}