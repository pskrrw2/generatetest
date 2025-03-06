if (typeof formModalComponent === 'undefined') {
    var formModalComponent = (function () {

        let component = $('#form-modal');
        let constants = {
            modalTitle: '.modal-title',
            modalBody: '.modal-body'
        };

        let replace = function (title, body) {
            component.find(constants.modalTitle).empty().html(title);
            component.find(constants.modalBody).empty().html(body);
        };

        let show = function (title, body) {
            replace(title, body);
            component.modal('show');
        };

        let hide = function () {
            component.find(constants.modalTitle).empty();
            component.find(constants.modalBody).empty();
            component.modal('hide');
        };

        return {
            show: show,
            hide: hide,
            replace: replace
        }
    })();
}
