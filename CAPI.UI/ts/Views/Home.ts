/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>

namespace VisTarsier.Views.Home {
    $(document).ready(() => {
        angularInit();
    });

    function angularInit() {
        const angularApp = angular.module("HomeView", []);
        
        angularApp.controller("HomeCtrl", ($scope, $http) => {
            $scope.LoadedSeries = new ViewModels.SeriesCollection();
            $scope.Viewports = new ViewModels.ViewportCollection();
            $scope.Viewports.addViewport(new ViewModels.Viewport());
            $scope.Viewports.addViewport(new ViewModels.Viewport());

            $scope.LinkButtonClass = "fa fa-chain";
            $scope.toggleLink = () => {
                if ($scope.LinkButtonClass === "fa fa-chain") $scope.LinkButtonClass = "fa fa-chain-broken";
                else $scope.LinkButtonClass = "fa fa-chain";
                $scope.Viewports.toggleLink();
            };
            
            $scope.mwDown = (viewportIndex: number) => {
                var controlKeyIsDown = (window.event as WheelEvent).ctrlKey;
                if (controlKeyIsDown) $scope.Viewports.displayNextSeries($scope.Viewports.viewportArray[viewportIndex]);
                else $scope.Viewports.displayNextImage($scope.Viewports.viewportArray[viewportIndex]);
            }
            $scope.mwUp = (viewportIndex: number) => {
                var controlKeyIsDown = (window.event as WheelEvent).ctrlKey;
                if (controlKeyIsDown) $scope.Viewports.displayPreviousSeries($scope.Viewports.viewportArray[viewportIndex]);
                else $scope.Viewports.displayPreviousImage($scope.Viewports.viewportArray[viewportIndex]);
            }

            $scope.LoadTwoSampleSeries = () => {
                const seriesDirNames = new Array("sample1", "sample2");
                $http.post("../api/Images/GetSampleSeries", seriesDirNames).then((response) => {
                    var resp = response.data;
                    var respData = resp.Data.Data as any[];
                    for (let i = 0; i < respData.length; i++) {
                        const filesList = respData[i].FilesList.map((s) => { return `img/${s}`; });
                        const seriesVm = new ViewModels.Series(respData[i].Name, "Dicom", filesList);
                        $scope.LoadedSeries.addSeries(seriesVm);
                    }
                });
                console.log($scope.LoadedSeries.seriesArray);
            };

            $scope.GetDicomSeries = () => {
                $http.get("../api/Images/GetDicomSeries").then((response) => {
                    var resp = response.data;
                    var respData = resp.Data.Data as any[];

                    respData.forEach((series) => {
                        const seriesVm = new ViewModels.Series(series.Name, "Dicom", series.FilesList);
                        $scope.LoadedSeries.addSeries(seriesVm);
                    });
                });
                console.log($scope.LoadedSeries.seriesArray);
            };

            $scope.SendSampleFileToPacs = () => {
                $http.get("../api/Images/SendToPacs").then((response) => { console.log(response); });
            };
            //$scope.Step1 = () => { $http.get("../api/ImageProcessing/step1").then((response) => { logResponse(response); }); }
            //$scope.RunAll = () => { $http.get("../api/ImageProcessing/runall").then((response) => { logResponse(response); }); }
        });

        angularApp.directive("ngMouseWheelUp", () => (scope, element, attrs) => {
            element.bind("DOMMouseScroll mousewheel onmousewheel", (event) => {

                // cross-browser wheel delta
                event = window.event || event; // old IE support
                var delta = Math.max(-1, Math.min(1, (event.wheelDelta || -event.detail)));

                if (delta > 0) {
                    scope.$apply(() => {
                        scope.$eval(attrs.ngMouseWheelUp);
                    });

                    // for IE
                    event.returnValue = false;
                    // for Chrome and Firefox
                    if (event.preventDefault) {
                        event.preventDefault();
                    }

                }
            });
        });

        angularApp.directive("ngMouseWheelDown", () => (scope, element, attrs) => {
            element.bind("DOMMouseScroll mousewheel onmousewheel", (event) => {

                // cross-browser wheel delta
                event = window.event || event; // old IE support
                var delta = Math.max(-1, Math.min(1, (event.wheelDelta || -event.detail)));

                if (delta < 0) {
                    scope.$apply(() => {
                        scope.$eval(attrs.ngMouseWheelDown);
                    });

                    // for IE
                    event.returnValue = false;
                    // for Chrome and Firefox
                    if (event.preventDefault) {
                        event.preventDefault();
                    }

                }
            });
        });
    }
}