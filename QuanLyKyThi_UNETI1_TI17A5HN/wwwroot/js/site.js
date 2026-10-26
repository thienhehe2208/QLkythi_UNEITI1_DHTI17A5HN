// Họ và tên: Phạm Quang Minh
// Mã sinh viên: 23103100201
// Nội dung thực hiện: Toast, data-confirm, data-auto-submit và nút mật khẩu theo quy ước nhóm.
(function () {
    "use strict";
    document.addEventListener("DOMContentLoaded", function () {
        document.querySelectorAll(".toast").forEach(function (element) {
            bootstrap.Toast.getOrCreateInstance(element, { delay: element.classList.contains("text-bg-danger") ? 10000 : 5000 }).show();
        });
        document.querySelectorAll("select[data-auto-submit]").forEach(function (select) {
            select.addEventListener("change", function () { select.form.requestSubmit(); });
        });
        document.querySelectorAll("[data-toggle-password]").forEach(function (button) {
            button.addEventListener("click", function () {
                var input = document.getElementById(button.dataset.togglePassword);
                if (!input) return;
                var show = input.type === "password";
                input.type = show ? "text" : "password";
                button.setAttribute("aria-label", show ? "Ẩn mật khẩu" : "Hiện mật khẩu");
                button.setAttribute("aria-pressed", String(show));
                var label = button.querySelector("[data-password-label]");
                if (label) label.textContent = show ? "Ẩn" : "Hiện";
                var icon = button.querySelector(".bi");
                if (icon) icon.className = show ? "bi bi-eye-slash" : "bi bi-eye";
            });
        });
        var modalElement = document.getElementById("modalXacNhan");
        if (!modalElement) return;
        var pendingForm = null;
        var modal = bootstrap.Modal.getOrCreateInstance(modalElement);
        document.addEventListener("submit", function (event) {
            var form = event.target;
            if (!form.matches("form[data-confirm]") || form.dataset.daXacNhan === "1") return;
            event.preventDefault();
            pendingForm = form;
            modalElement.querySelector(".modal-title").textContent = form.dataset.confirmTitle || "Xác nhận thao tác";
            modalElement.querySelector(".modal-body p").textContent = form.dataset.confirm;
            var button = modalElement.querySelector("[data-modal-ok]");
            button.textContent = form.dataset.confirmOk || "Đồng ý";
            button.className = "btn " + (form.dataset.confirmVariant || "btn-primary");
            modal.show();
        });
        modalElement.querySelector("[data-modal-ok]").addEventListener("click", function () {
            if (!pendingForm) return;
            var form = pendingForm;
            pendingForm = null;
            form.dataset.daXacNhan = "1";
            modal.hide();
            form.requestSubmit();
            delete form.dataset.daXacNhan;
        });
        modalElement.addEventListener("hidden.bs.modal", function () { pendingForm = null; });
    });
})();
