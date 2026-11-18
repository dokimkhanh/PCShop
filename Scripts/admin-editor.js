(function (window, $) {
    function uploadEditorImages(files, editor, onDone) {
        if (!files || files.length === 0) {
            return;
        }
        var imageFiles = [];
        for (var i = 0; i < files.length; i++) {
            if (files[i] && files[i].type && files[i].type.indexOf('image/') === 0) {
                imageFiles.push(files[i]);
            }
        }
        if (imageFiles.length === 0) {
            if (window.toastr) {
                toastr.warning('Vui lòng chọn tệp hình ảnh hợp lệ (JPG, PNG, WEBP, GIF).');
            }
            return;
        }

        var formData = new FormData();
        for (var j = 0; j < imageFiles.length; j++) {
            formData.append('files', imageFiles[j]);
        }

        var token = $('input[name="__RequestVerificationToken"]').first().val();
        if (window.toastr) {
            toastr.info('Đang tải ' + imageFiles.length + ' ảnh vào nội dung...');
        }

        $.ajax({
            url: '/Admin/Upload/Image',
            type: 'POST',
            data: formData,
            processData: false,
            contentType: false,
            headers: token ? { '__RequestVerificationToken': token } : {},
            success: function (res) {
                if (res && res.success && res.urls && res.urls.length > 0) {
                    var html = '';
                    res.urls.forEach(function (url) {
                        html += '<p><img src="' + url + '" alt="" style="max-width:100%;height:auto;" /></p>';
                    });
                    editor.insertHtml(html);
                    if (window.toastr) {
                        toastr.success('Đã chèn ' + res.urls.length + ' ảnh vào nội dung.');
                    }
                } else {
                    if (window.toastr) {
                        toastr.error((res && res.message) || 'Không thể tải ảnh lên.');
                    }
                }
            },
            error: function () {
                if (window.toastr) {
                    toastr.error('Lỗi kết nối máy chủ khi tải ảnh.');
                }
            },
            complete: function () {
                if (typeof onDone === 'function') {
                    onDone();
                }
            }
        });
    }

    window.initAdminEditor = function (elementId, insertBtnSelector) {
        if (typeof CKEDITOR === 'undefined') {
            return null;
        }

        var editor = CKEDITOR.replace(elementId, {
            customConfig: '/Content/ckeditor/config.js?v=20261118',
            language: 'vi',
            enterMode: CKEDITOR.ENTER_BR,
            filebrowserBrowseUrl: '/Admin/Upload/Browse',
            filebrowserImageBrowseUrl: '/Admin/Upload/Browse',
            filebrowserUploadUrl: '/Admin/Upload/CkEditorImage',
            filebrowserImageUploadUrl: '/Admin/Upload/CkEditorImage',
            filebrowserWindowWidth: '960',
            filebrowserWindowHeight: '640',
            extraAllowedContent: 'img[*]{*}(*);span[*]{*}(*);iframe[*]{*}(*)'
        });

        editor.on('contentDom', function () {
            var editable = editor.editable();

            editable.attachListener(editable, 'paste', function (evt) {
                var nativeEvent = evt.data.$;
                var clipboard = nativeEvent.clipboardData || window.clipboardData;
                if (!clipboard || !clipboard.items) {
                    return;
                }
                var pastedImages = [];
                for (var i = 0; i < clipboard.items.length; i++) {
                    var item = clipboard.items[i];
                    if (item.kind === 'file' && item.type && item.type.indexOf('image/') === 0) {
                        var file = item.getAsFile();
                        if (file) {
                            pastedImages.push(file);
                        }
                    }
                }
                if (pastedImages.length > 0) {
                    nativeEvent.preventDefault();
                    uploadEditorImages(pastedImages, editor);
                }
            });

            editable.attachListener(editable, 'dragover', function (evt) {
                evt.data.$.preventDefault();
            });

            editable.attachListener(editable, 'drop', function (evt) {
                var nativeEvent = evt.data.$;
                var dt = nativeEvent.dataTransfer;
                if (!dt || !dt.files || dt.files.length === 0) {
                    return;
                }
                var droppedImages = [];
                for (var i = 0; i < dt.files.length; i++) {
                    if (dt.files[i].type && dt.files[i].type.indexOf('image/') === 0) {
                        droppedImages.push(dt.files[i]);
                    }
                }
                if (droppedImages.length > 0) {
                    nativeEvent.preventDefault();
                    uploadEditorImages(droppedImages, editor);
                }
            });
        });

        if (insertBtnSelector) {
            var $btn = $(insertBtnSelector);
            if ($btn.length > 0) {
                var inputId = elementId + '_quickImgInput';
                var $fileInput = $('<input type="file" accept="image/*" multiple style="display:none;" />').attr('id', inputId);
                $btn.after($fileInput);

                $btn.on('click', function () {
                    $fileInput.trigger('click');
                });

                $fileInput.on('change', function () {
                    var files = this.files;
                    if (files && files.length > 0) {
                        $btn.prop('disabled', true);
                        uploadEditorImages(files, editor, function () {
                            $btn.prop('disabled', false);
                            $fileInput.val('');
                        });
                    }
                });
            }
        }

        var $textarea = $('#' + elementId);
        $textarea.closest('form').on('submit', function () {
            if (editor) {
                editor.updateElement();
            }
        });

        return editor;
    };
})(window, jQuery);
