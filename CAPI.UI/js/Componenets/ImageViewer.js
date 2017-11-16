var VisTarsier;
(function (VisTarsier) {
    var Components1;
    (function (Components1) {
        var ImageViewer = (function () {
            function ImageViewer(elems) {
                var _this = this;
                this.totalNoOfImages = 160;
                elems.forEach(function (elem) {
                    $("#" + elem.id).bind("mousewheel DOMMouseScroll", function (event) {
                        var self = event.target;
                        var other = $(".imageViewerContainersRow").find(".imageViewer").not("#" + self.id);
                        var imgNum = parseInt($(self).data("imgNumber"));
                        var imagesAreLinked = $("#btnLinkNegAndPos").data("linked");
                        if (event.originalEvent.wheelDelta > 0 || event.originalEvent.detail < 0) {
                            // Up
                            if (imgNum > 0) {
                                var targetsArr = new Array(self);
                                if (imagesAreLinked)
                                    targetsArr.push(other[0]);
                                //displayPrevImage(targetsArr);
                            }
                        }
                        else {
                            // Down
                            if (imgNum < _this.totalNoOfImages) {
                                var targetsArr = new Array(self);
                                if (imagesAreLinked)
                                    targetsArr.push(other[0]);
                                //displayNextImage(targetsArr); 
                            }
                        }
                        return false;
                    });
                });
            }
            return ImageViewer;
        }());
        Components1.ImageViewer = ImageViewer;
    })(Components1 = VisTarsier.Components1 || (VisTarsier.Components1 = {}));
})(VisTarsier || (VisTarsier = {}));
//# sourceMappingURL=D:/Dropbox/Projects/VT/VT-App/D:/Dropbox/Projects/CAPI/Application/CAPI.UI/js/Componenets/ImageViewer.js.map