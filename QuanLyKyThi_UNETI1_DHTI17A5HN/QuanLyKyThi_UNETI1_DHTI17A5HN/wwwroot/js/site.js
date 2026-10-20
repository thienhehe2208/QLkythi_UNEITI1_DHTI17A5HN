// ============================================================
// Họ và tên: Nguyễn Đắc Tiến
// Mã sinh viên: 23103100232
// Nội dung thực hiện: Tiện ích giao diện dùng chung:
//  - Tự hiện thông báo dạng toast.
//  - Hộp xác nhận cho form có thuộc tính data-confirm (khóa, xóa, đổi trạng thái...).
//  - Nút ẩn/hiện mật khẩu.
//  - Tự gửi form lọc khi đổi giá trị select có data-auto-submit.
// ============================================================
(function () {
    "use strict";

    document.addEventListener("DOMContentLoaded", function () {
        // Toast
        document.querySelectorAll(".toast").forEach(function (el) {
            bootstrap.Toast.getOrCreateInstance(el, { delay: 5000 }).show();
        });

        // Tooltip cho các nút chỉ có icon
        document.querySelectorAll("[data-bs-toggle='tooltip']").forEach(function (el) {
            bootstrap.Tooltip.getOrCreateInstance(el);
        });

        // Ẩn/hiện mật khẩu
        document.querySelectorAll("[data-toggle-password]").forEach(function (btn) {
            btn.addEventListener("click", function () {
                var input = document.getElementById(btn.getAttribute("data-toggle-password"));
                if (!input) return;
                var hien = input.type === "password";
                input.type = hien ? "text" : "password";
                btn.querySelector(".bi").className = hien ? "bi bi-eye-slash" : "bi bi-eye";
                btn.setAttribute("aria-label", hien ? "Ẩn mật khẩu" : "Hiện mật khẩu");
            });
        });

        // Tự gửi form lọc
        document.querySelectorAll("select[data-auto-submit]").forEach(function (sel) {
            sel.addEventListener("change", function () { sel.form.submit(); });
        });

        // Hộp xác nhận
        var modalEl = document.getElementById("modalXacNhan");
        if (!modalEl) return;
        var modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        var formCho = null;

        document.addEventListener("submit", function (e) {
            var form = e.target;
            if (!form.matches("form[data-confirm]") || form.dataset.daXacNhan === "1") return;
            e.preventDefault();
            formCho = form;
            modalEl.querySelector(".modal-title").textContent = form.dataset.confirmTitle || "Xác nhận thao tác";
            modalEl.querySelector(".modal-body p").textContent = form.dataset.confirm;
            var nut = modalEl.querySelector("[data-modal-ok]");
            nut.textContent = form.dataset.confirmOk || "Đồng ý";
            nut.className = "btn " + (form.dataset.confirmVariant || "btn-primary");
            modal.show();
        });

        modalEl.querySelector("[data-modal-ok]").addEventListener("click", function () {
            if (!formCho) return;
            formCho.dataset.daXacNhan = "1";
            modal.hide();
            formCho.requestSubmit ? formCho.requestSubmit() : formCho.submit();
        });
    });
})();
