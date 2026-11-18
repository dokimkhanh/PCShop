CKEDITOR.editorConfig = function (config) {
    config.language = 'vi';
    config.enterMode = CKEDITOR.ENTER_BR;
    config.toolbar = 'Full';
    config.filebrowserBrowseUrl = '/Admin/Upload/Browse';
    config.filebrowserImageBrowseUrl = '/Admin/Upload/Browse';
    config.filebrowserUploadUrl = '/Admin/Upload/CkEditorImage';
    config.filebrowserImageUploadUrl = '/Admin/Upload/CkEditorImage';
    config.filebrowserWindowWidth = '960';
    config.filebrowserWindowHeight = '640';
};
