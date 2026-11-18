(function (window, $) {
    var MAX_FILE_SIZE = 10 * 1024 * 1024;
    var ALLOWED_EXTS = ['.jpg', '.jpeg', '.jfif', '.png', '.gif', '.webp'];

    function formatSize(bytes) {
        if (!bytes || bytes <= 0) {
            return '0 KB';
        }
        if (bytes >= 1024 * 1024) {
            return (bytes / (1024 * 1024)).toFixed(1).replace('.', ',') + ' MB';
        }
        return Math.max(1, Math.round(bytes / 1024)) + ' KB';
    }

    function splitMiddleName(name) {
        if (!name || name.length <= 24) {
            return { head: name || '', tail: '', truncated: false };
        }
        var dotIdx = name.lastIndexOf('.');
        var ext = dotIdx > 0 ? name.substring(dotIdx) : '';
        var base = dotIdx > 0 ? name.substring(0, dotIdx) : name;
        if (base.length <= 10) {
            return { head: name, tail: '', truncated: false };
        }
        var keepTail = base.substring(base.length - 8) + ext;
        var head = base.substring(0, base.length - 8);
        return { head: head, tail: keepTail, truncated: true };
    }

    function getExtension(fileName) {
        if (!fileName) {
            return '';
        }
        var idx = fileName.lastIndexOf('.');
        return idx >= 0 ? fileName.substring(idx).toLowerCase() : '';
    }

    window.initProductMediaGallery = function (options) {
        var $root = $(options.rootSelector);
        if ($root.length === 0) {
            return;
        }

        var galleryImages = [];
        var primaryIndex = 0;
        var queueItems = [];
        var queueSeq = 0;
        var activeUploads = 0;
        var dragSourceIdx = null;
        var lightboxIdx = 0;
        var $submitBtn = options.submitButtonSelector ? $(options.submitButtonSelector) : $();

        var initialData = options.initialImages || [];
        initialData.forEach(function (entry) {
            var url = typeof entry === 'string' ? entry : (entry && entry.url);
            if (url && galleryImages.indexOf(url) === -1) {
                galleryImages.push(url);
                if (typeof entry === 'object' && entry.isDefault) {
                    primaryIndex = galleryImages.length - 1;
                }
            }
        });
        if (typeof options.initialPrimaryIndex === 'number' && options.initialPrimaryIndex >= 0 && options.initialPrimaryIndex < galleryImages.length) {
            primaryIndex = options.initialPrimaryIndex;
        }

        function ensureLightboxModal() {
            if ($('#pmgLightboxModal').length > 0) {
                return $('#pmgLightboxModal');
            }
            var html = ''
                + '<div class="modal fade" id="pmgLightboxModal" tabindex="-1" role="dialog" aria-hidden="true">'
                + '  <div class="modal-dialog modal-lg modal-dialog-centered" role="document">'
                + '    <div class="modal-content">'
                + '      <div class="modal-header">'
                + '        <h5 class="modal-title"><span id="pmgLightboxTitle">Xem ảnh sản phẩm</span> <span id="pmgLightboxCounter" class="badge-clean badge-clean-neutral ml-2 tabular-nums"></span></h5>'
                + '        <button type="button" class="close" data-dismiss="modal" aria-label="Đóng"><span aria-hidden="true">&times;</span></button>'
                + '      </div>'
                + '      <div class="modal-body">'
                + '        <div class="pmg-lightbox-stage"><img id="pmgLightboxImg" src="" alt="Xem trước ảnh sản phẩm" /></div>'
                + '      </div>'
                + '      <div class="modal-footer d-flex flex-wrap align-items-center justify-content-between" style="gap: 8px;">'
                + '        <div class="d-flex align-items-center" style="gap: 6px;">'
                + '          <button type="button" class="btn btn-default btn-sm" id="pmgLightboxPrev"><i class="fas fa-chevron-left"></i> Trước</button>'
                + '          <button type="button" class="btn btn-default btn-sm" id="pmgLightboxNext">Sau <i class="fas fa-chevron-right"></i></button>'
                + '        </div>'
                + '        <div class="d-flex align-items-center" style="gap: 8px;">'
                + '          <button type="button" class="btn btn-default btn-sm" id="pmgLightboxSetPrimary"><i class="fas fa-star"></i> Đặt ảnh chính</button>'
                + '          <button type="button" class="btn btn-danger btn-sm" id="pmgLightboxDelete"><i class="fas fa-trash-alt"></i> Xoá ảnh</button>'
                + '        </div>'
                + '      </div>'
                + '    </div>'
                + '  </div>'
                + '</div>';
            $('body').append(html);
            return $('#pmgLightboxModal');
        }

        function syncLightboxView() {
            if (galleryImages.length === 0) {
                $('#pmgLightboxModal').modal('hide');
                return;
            }
            if (lightboxIdx < 0) {
                lightboxIdx = galleryImages.length - 1;
            }
            if (lightboxIdx >= galleryImages.length) {
                lightboxIdx = 0;
            }
            var url = galleryImages[lightboxIdx];
            var isPrimary = lightboxIdx === primaryIndex;
            $('#pmgLightboxImg').attr('src', url);
            $('#pmgLightboxTitle').text(isPrimary ? 'Ảnh chính sản phẩm' : 'Ảnh sản phẩm');
            $('#pmgLightboxCounter').text((lightboxIdx + 1) + ' / ' + galleryImages.length);
            $('#pmgLightboxSetPrimary').toggleClass('d-none', isPrimary);
            $('#pmgLightboxPrev, #pmgLightboxNext').prop('disabled', galleryImages.length <= 1);
        }

        function openLightbox(idx) {
            var $modal = ensureLightboxModal();
            lightboxIdx = idx;
            syncLightboxView();

            $('#pmgLightboxPrev').off('click').on('click', function () {
                lightboxIdx--;
                syncLightboxView();
            });
            $('#pmgLightboxNext').off('click').on('click', function () {
                lightboxIdx++;
                syncLightboxView();
            });
            $('#pmgLightboxSetPrimary').off('click').on('click', function () {
                setPrimary(lightboxIdx);
                lightboxIdx = 0;
                syncLightboxView();
            });
            $('#pmgLightboxDelete').off('click').on('click', function () {
                removeImage(lightboxIdx);
                syncLightboxView();
            });

            $modal.modal('show');
        }

        function moveImage(fromIdx, toIdx) {
            if (fromIdx === toIdx || fromIdx < 0 || toIdx < 0 || fromIdx >= galleryImages.length || toIdx >= galleryImages.length) {
                return;
            }
            var moved = galleryImages.splice(fromIdx, 1)[0];
            galleryImages.splice(toIdx, 0, moved);
            primaryIndex = 0;
            renderGallery();
        }

        function setPrimary(idx) {
            if (idx < 0 || idx >= galleryImages.length) {
                return;
            }
            if (idx !== 0) {
                var chosen = galleryImages.splice(idx, 1)[0];
                galleryImages.unshift(chosen);
            }
            primaryIndex = 0;
            renderGallery();
        }

        function removeImage(idx) {
            if (idx < 0 || idx >= galleryImages.length) {
                return;
            }
            galleryImages.splice(idx, 1);
            if (galleryImages.length === 0) {
                primaryIndex = 0;
            } else if (primaryIndex === idx) {
                primaryIndex = 0;
            } else if (primaryIndex > idx) {
                primaryIndex--;
            }
            renderGallery();
        }

        function syncSubmitState() {
            if ($submitBtn.length > 0) {
                $submitBtn.prop('disabled', activeUploads > 0);
            }
        }

        function renderQueue() {
            var $queue = $root.find('.pmg-queue');
            if (queueItems.length === 0) {
                $queue.hide().empty();
                return;
            }
            $queue.show().empty();

            queueItems.forEach(function (item) {
                var $li = $('<li class="pmg-queue-item"></li>');
                $li.append('<div class="pmg-queue-icon"><i class="far fa-file-image"></i></div>');

                var $body = $('<div class="pmg-queue-body"></div>');
                var $top = $('<div class="pmg-queue-top"></div>');
                var parts = splitMiddleName(item.name);
                var $name = $('<div class="pmg-queue-name"></div>');
                if (parts.truncated) {
                    $name.attr('title', item.name);
                    $name.append($('<span class="pmg-queue-name-head"></span>').text(parts.head));
                    $name.append($('<span class="pmg-queue-name-tail"></span>').text(parts.tail));
                } else {
                    $name.append($('<span class="pmg-queue-name-head"></span>').text(item.name));
                }
                $top.append($name);

                if (item.status === 'uploading') {
                    $top.append($('<span class="pmg-queue-pct"></span>').text(item.progress + '%'));
                }
                $body.append($top);

                var $sub = $('<div class="pmg-queue-sub"></div>');
                if (item.status === 'uploading') {
                    var $track = $('<div class="pmg-progress-track" role="progressbar" aria-valuemin="0" aria-valuemax="100"></div>')
                        .attr('aria-valuenow', item.progress);
                    $track.append($('<div class="pmg-progress-fill"></div>').css('width', item.progress + '%'));
                    $sub.append($track);
                } else if (item.status === 'done') {
                    $sub.append('<i class="fas fa-check-circle" style="color: var(--success);"></i>');
                    $sub.append($('<span></span>').text('Đã tải xong · ' + formatSize(item.size)));
                } else if (item.status === 'error') {
                    $sub.append($('<span style="color: var(--danger);"></span>').text(item.errorMessage || 'Tải ảnh thất bại'));
                    if (item.retryable && item.file) {
                        $sub.append('<span style="color: var(--muted);">·</span>');
                        var $retry = $('<button type="button" class="pmg-retry-btn">Thử lại</button>');
                        $retry.on('click', function () {
                            startSingleUpload(item);
                        });
                        $sub.append($retry);
                    }
                } else {
                    $sub.text('Đang chờ · ' + formatSize(item.size));
                }
                $body.append($sub);
                $li.append($body);

                var $action = $('<div class="pmg-queue-action"></div>');
                if (item.status === 'uploading') {
                    var $abort = $('<button type="button" class="btn-icon" aria-label="Huỷ tải"><i class="fas fa-times"></i></button>');
                    $abort.on('click', function () {
                        if (item.xhr) {
                            item.xhr.abort();
                        }
                        queueItems = queueItems.filter(function (q) { return q.id !== item.id; });
                        renderQueue();
                    });
                    $action.append($abort);
                } else if (item.status === 'error') {
                    var $dismiss = $('<button type="button" class="btn-icon" aria-label="Bỏ tệp lỗi"><i class="fas fa-times"></i></button>');
                    $dismiss.on('click', function () {
                        queueItems = queueItems.filter(function (q) { return q.id !== item.id; });
                        renderQueue();
                    });
                    $action.append($dismiss);
                }
                $li.append($action);
                $queue.append($li);
            });
        }

        function startSingleUpload(item, onBatchDone) {
            item.status = 'uploading';
            item.progress = 0;
            item.errorMessage = '';
            activeUploads++;
            syncSubmitState();
            renderQueue();

            var formData = new FormData();
            var token = $('input[name="__RequestVerificationToken"]').first().val();
            if (token) {
                formData.append('__RequestVerificationToken', token);
            }
            formData.append('files', item.file);

            item.xhr = $.ajax({
                url: '/Admin/Upload/Image',
                type: 'POST',
                data: formData,
                processData: false,
                contentType: false,
                xhr: function () {
                    var xhr = new window.XMLHttpRequest();
                    xhr.upload.addEventListener('progress', function (evt) {
                        if (evt.lengthComputable) {
                            item.progress = Math.min(99, Math.round((evt.loaded / evt.total) * 100));
                            renderQueue();
                        }
                    }, false);
                    return xhr;
                },
                success: function (res) {
                    if (res && res.success && res.urls && res.urls.length > 0) {
                        item.status = 'done';
                        item.progress = 100;
                        res.urls.forEach(function (u) {
                            if (galleryImages.indexOf(u) === -1) {
                                galleryImages.push(u);
                            }
                        });
                        renderGallery();
                    } else {
                        item.status = 'error';
                        item.retryable = true;
                        item.errorMessage = (res && res.message) || 'Không thể tải ảnh lên';
                    }
                },
                error: function (xhr, statusText) {
                    if (statusText === 'abort') {
                        return;
                    }
                    item.status = 'error';
                    item.retryable = true;
                    item.errorMessage = (xhr.responseJSON && xhr.responseJSON.message)
                        ? xhr.responseJSON.message
                        : 'Mất kết nối máy chủ (HTTP ' + xhr.status + ')';
                },
                complete: function () {
                    item.xhr = null;
                    activeUploads = Math.max(0, activeUploads - 1);
                    syncSubmitState();
                    renderQueue();
                    if (typeof onBatchDone === 'function') {
                        onBatchDone(item);
                    }
                }
            });
        }

        function handleFiles(fileList) {
            if (!fileList || fileList.length === 0) {
                return;
            }
            var batch = [];
            var validToUpload = [];

            for (var i = 0; i < fileList.length; i++) {
                var file = fileList[i];
                var ext = getExtension(file.name);
                var qItem = {
                    id: ++queueSeq,
                    file: file,
                    name: file.name,
                    size: file.size,
                    progress: 0,
                    status: 'queued',
                    retryable: false,
                    errorMessage: '',
                    xhr: null
                };

                if (ALLOWED_EXTS.indexOf(ext) === -1) {
                    qItem.status = 'error';
                    qItem.retryable = false;
                    qItem.errorMessage = 'Không hỗ trợ định dạng ' + (ext || 'này');
                } else if (file.size > MAX_FILE_SIZE) {
                    qItem.status = 'error';
                    qItem.retryable = false;
                    qItem.errorMessage = 'Quá 10 MB (tệp nặng ' + formatSize(file.size) + ')';
                } else {
                    validToUpload.push(qItem);
                }
                batch.push(qItem);
                queueItems.push(qItem);
            }

            renderQueue();
            if (validToUpload.length === 0) {
                return;
            }

            var remaining = validToUpload.length;
            var batchSuccessCount = 0;
            var batchHasError = batch.length !== validToUpload.length;

            validToUpload.forEach(function (qItem) {
                startSingleUpload(qItem, function (doneItem) {
                    remaining--;
                    if (doneItem.status === 'done') {
                        batchSuccessCount++;
                    } else {
                        batchHasError = true;
                    }
                    if (remaining <= 0) {
                        if (!batchHasError && batchSuccessCount > 0 && window.toastr) {
                            toastr.success('Đã tải lên ' + batchSuccessCount + ' ảnh sản phẩm');
                        }
                        setTimeout(function () {
                            queueItems = queueItems.filter(function (q) { return q.status !== 'done'; });
                            renderQueue();
                        }, 2600);
                    }
                });
            });
        }

        function bindDropTarget($el, titleSelector, defaultText) {
            $el.on('dragover dragenter', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $el.addClass('is-dragover');
                if (titleSelector) {
                    $el.find(titleSelector).text('Thả ảnh để tải lên');
                }
            });
            $el.on('dragleave', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $el.removeClass('is-dragover');
                if (titleSelector) {
                    $el.find(titleSelector).text(defaultText);
                }
            });
            $el.on('drop', function (e) {
                e.preventDefault();
                e.stopPropagation();
                $el.removeClass('is-dragover');
                if (titleSelector) {
                    $el.find(titleSelector).text(defaultText);
                }
                var dt = e.originalEvent.dataTransfer;
                if (dt && dt.files && dt.files.length > 0) {
                    handleFiles(dt.files);
                }
            });
        }

        function renderGallery() {
            var $countBadge = $root.find('.pmg-count-badge');
            var $headerAddBtn = $root.find('.pmg-header-add');
            var $emptyZone = $root.find('.pmg-empty-dropzone');
            var $grid = $root.find('.pmg-grid');
            var $defaultInput = $root.find('input[name="rdDefault"]');
            var $hiddenInputs = $root.find('.pmg-hidden-inputs');

            $hiddenInputs.empty();

            if (galleryImages.length === 0) {
                $countBadge.text('0 ảnh');
                $headerAddBtn.addClass('d-none');
                $defaultInput.val(0);
                $grid.hide().empty();
                $emptyZone.show();
                return;
            }

            if (primaryIndex < 0 || primaryIndex >= galleryImages.length) {
                primaryIndex = 0;
            }

            if (primaryIndex !== 0) {
                var chosen = galleryImages.splice(primaryIndex, 1)[0];
                galleryImages.unshift(chosen);
                primaryIndex = 0;
            }

            $countBadge.text(galleryImages.length + ' ảnh');
            $headerAddBtn.removeClass('d-none');
            $defaultInput.val(primaryIndex);
            $emptyZone.hide();
            $grid.show().empty();

            galleryImages.forEach(function (url, idx) {
                $hiddenInputs.append($('<input type="hidden" name="listImage" />').val(url));

                var isPrimary = idx === primaryIndex;
                var $tile = $('<div class="pmg-tile" draggable="true"></div>').attr('data-idx', idx);
                if (isPrimary) {
                    $tile.addClass('is-primary');
                }

                $tile.on('dragstart', function (e) {
                    dragSourceIdx = idx;
                    $tile.addClass('is-dragging');
                    e.originalEvent.dataTransfer.effectAllowed = 'move';
                });
                $tile.on('dragend', function () {
                    dragSourceIdx = null;
                    $grid.find('.pmg-tile').removeClass('is-dragging is-drop-target');
                });
                $tile.on('dragover', function (e) {
                    if (dragSourceIdx !== null && dragSourceIdx !== idx) {
                        e.preventDefault();
                        $tile.addClass('is-drop-target');
                    }
                });
                $tile.on('dragleave', function () {
                    $tile.removeClass('is-drop-target');
                });
                $tile.on('drop', function (e) {
                    if (dragSourceIdx !== null && dragSourceIdx !== idx) {
                        e.preventDefault();
                        e.stopPropagation();
                        moveImage(dragSourceIdx, idx);
                    }
                });

                var $thumbBox = $('<div class="pmg-thumb-box" title="Nhấn để xem lớn"></div>');
                $thumbBox.append($('<img />').attr('src', url).attr('alt', isPrimary ? 'Ảnh chính sản phẩm' : ('Ảnh sản phẩm ' + (idx + 1))));
                if (isPrimary) {
                    $thumbBox.append('<span class="pmg-primary-pill"><i class="fas fa-star"></i> Ảnh chính</span>');
                } else {
                    $thumbBox.append($('<span class="pmg-index-pill"></span>').text(idx + 1));
                }
                $thumbBox.on('click', function () {
                    openLightbox(idx);
                });
                $tile.append($thumbBox);

                var $bar = $('<div class="pmg-tile-bar"></div>');
                if (isPrimary) {
                    var $zoomBtn = $('<button type="button" class="pmg-bar-btn is-active"><i class="fas fa-search-plus"></i><span>Xem lớn</span></button>');
                    $zoomBtn.on('click', function () { openLightbox(idx); });
                    $bar.append($zoomBtn);
                } else {
                    var $setMainBtn = $('<button type="button" class="pmg-bar-btn" title="Đặt làm ảnh đại diện"><i class="far fa-star"></i><span>Ảnh chính</span></button>');
                    $setMainBtn.on('click', function () { setPrimary(idx); });
                    $bar.append($setMainBtn);
                }

                var $rightGroup = $('<div class="d-flex align-items-center" style="gap: 2px;"></div>');
                if (!isPrimary) {
                    var $previewIcon = $('<button type="button" class="pmg-bar-btn" title="Xem lớn" aria-label="Xem lớn"><i class="fas fa-expand-alt"></i></button>');
                    $previewIcon.on('click', function () { openLightbox(idx); });
                    $rightGroup.append($previewIcon);
                }
                var $delBtn = $('<button type="button" class="pmg-bar-btn pmg-bar-btn-danger" title="Xoá ảnh" aria-label="Xoá ảnh"><i class="fas fa-trash-alt"></i></button>');
                $delBtn.on('click', function () { removeImage(idx); });
                $rightGroup.append($delBtn);
                $bar.append($rightGroup);

                $tile.append($bar);
                $grid.append($tile);
            });

            var inputId = $root.find('.pmg-file-input').attr('id');
            var $addTile = $('<label class="pmg-add-tile"></label>').attr('for', inputId);
            $addTile.append('<div class="pmg-add-tile-icon"><i class="fas fa-plus"></i></div>');
            $addTile.append('<div class="pmg-add-tile-label" style="font-size: 13px; font-weight: 500;">Thêm ảnh</div>');
            $addTile.append('<div style="font-size: 11.5px; color: var(--muted); margin-top: 2px;">Kéo thả hoặc chọn</div>');
            bindDropTarget($addTile, '.pmg-add-tile-label', 'Thêm ảnh');
            $grid.append($addTile);
        }

        var $fileInput = $root.find('.pmg-file-input');
        $fileInput.on('change', function () {
            if (this.files && this.files.length > 0) {
                handleFiles(this.files);
                $(this).val('');
            }
        });

        bindDropTarget($root.find('.pmg-empty-dropzone'), '.pmg-drop-title-drag', 'Kéo thả hình ảnh sản phẩm vào đây');
        renderGallery();
    };
})(window, jQuery);
