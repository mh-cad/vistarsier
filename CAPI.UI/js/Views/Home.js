/// <reference path="../../Scripts/typings/jquery/jquery.d.ts"/>
var VisTarsier;
(function (VisTarsier) {
    var Views;
    (function (Views) {
        var Home;
        (function (Home) {
            $(document).ready(function () {
                angularInit();
            });
            function angularInit() {
                var angularApp = angular.module("HomeView", []);
                angularApp.controller("HomeCtrl", function ($scope, $http) {
                    $scope.LoadedSeries = new VisTarsier.ViewModels.SeriesCollection();
                    $scope.Viewports = new VisTarsier.ViewModels.ViewportCollection();
                    $scope.Viewports.addViewport(new VisTarsier.ViewModels.Viewport());
                    $scope.Viewports.addViewport(new VisTarsier.ViewModels.Viewport());
                    $scope.LinkButtonClass = "fa fa-chain";
                    $scope.toggleLink = function () {
                        if ($scope.LinkButtonClass === "fa fa-chain")
                            $scope.LinkButtonClass = "fa fa-chain-broken";
                        else
                            $scope.LinkButtonClass = "fa fa-chain";
                        $scope.Viewports.toggleLink();
                    };
                    $scope.mwDown = function (viewportIndex) {
                        var controlKeyIsDown = window.event.ctrlKey;
                        if (controlKeyIsDown)
                            $scope.Viewports.displayNextSeries($scope.Viewports.viewportArray[viewportIndex]);
                        else
                            $scope.Viewports.displayNextImage($scope.Viewports.viewportArray[viewportIndex]);
                    };
                    $scope.mwUp = function (viewportIndex) {
                        var controlKeyIsDown = window.event.ctrlKey;
                        if (controlKeyIsDown)
                            $scope.Viewports.displayPreviousSeries($scope.Viewports.viewportArray[viewportIndex]);
                        else
                            $scope.Viewports.displayPreviousImage($scope.Viewports.viewportArray[viewportIndex]);
                    };
                    $scope.LoadTwoSampleSeries = function () {
                        var seriesDirNames = new Array("sample1", "sample2");
                        $http.post("../api/Images/GetSampleSeries", seriesDirNames).then(function (response) {
                            var resp = response.data;
                            var respData = resp.Data.Data;
                            for (var i = 0; i < respData.length; i++) {
                                var filesList = respData[i].FilesList.map(function (s) { return "img/" + s; });
                                var seriesVm = new VisTarsier.ViewModels.Series(respData[i].Name, "Dicom", filesList);
                                $scope.LoadedSeries.addSeries(seriesVm);
                            }
                        });
                        console.log($scope.LoadedSeries.seriesArray);
                    };
                    $scope.GetDicomSeries = function () {
                        $http.get("../api/Images/GetDicomSeries").then(function (response) {
                            var resp = response.data;
                            var respData = resp.Data.Data;
                            respData.forEach(function (series) {
                                var seriesVm = new VisTarsier.ViewModels.Series(series.Name, "Dicom", series.FilesList);
                                $scope.LoadedSeries.addSeries(seriesVm);
                            });
                        });
                        console.log($scope.LoadedSeries.seriesArray);
                    };
                    $scope.SendSampleFileToPacs = function () {
                        $http.get("../api/Images/SendToPacs").then(function (response) { console.log(response); });
                    };
                    //$scope.Step1 = () => { $http.get("../api/ImageProcessing/step1").then((response) => { logResponse(response); }); }
                    //$scope.RunAll = () => { $http.get("../api/ImageProcessing/runall").then((response) => { logResponse(response); }); }
                });
                angularApp.directive("ngMouseWheelUp", function () { return function (scope, element, attrs) {
                    element.bind("DOMMouseScroll mousewheel onmousewheel", function (event) {
                        // cross-browser wheel delta
                        event = window.event || event; // old IE support
                        var delta = Math.max(-1, Math.min(1, (event.wheelDelta || -event.detail)));
                        if (delta > 0) {
                            scope.$apply(function () {
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
                }; });
                angularApp.directive("ngMouseWheelDown", function () { return function (scope, element, attrs) {
                    element.bind("DOMMouseScroll mousewheel onmousewheel", function (event) {
                        // cross-browser wheel delta
                        event = window.event || event; // old IE support
                        var delta = Math.max(-1, Math.min(1, (event.wheelDelta || -event.detail)));
                        if (delta < 0) {
                            scope.$apply(function () {
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
                }; });
            }
        })(Home = Views.Home || (Views.Home = {}));
    })(Views = VisTarsier.Views || (VisTarsier.Views = {}));
})(VisTarsier || (VisTarsier = {}));
//# sourceMappingURL=D:/Dropbox/Projects/VT/VT-App/D:/Dropbox/Projects/CAPI/Application/CAPI.UI/js/Views/Home.js.map