
document.addEventListener("DOMContentLoaded", function () {
    function loadPage(page) {
        const form = document.getElementById("filterForm");
        const formData = new FormData(form);
        const params = new URLSearchParams(formData);
        params.append("page", page);

        fetch('/Shop/FilterAjax?' + params.toString())
            .then(response => response.text())
            .then(html => {
                document.getElementById("productList").innerHTML = html;
                renderPagination(page);
            });
    }

    function renderPagination(currentPage) {
        let paginationHtml = '';
        const totalPages = parseInt(document.getElementById("TotalPages").value || 1);

        for (let i = 1; i <= totalPages; i++) {
            paginationHtml += `<button class='page-btn' data-page='${i}'>${i}</button> `;
        }
        document.getElementById("pagination").innerHTML = paginationHtml;

        document.querySelectorAll(".page-btn").forEach(btn => {
            btn.addEventListener("click", function () {
                loadPage(this.dataset.page);
            });
        });
    }

    document.getElementById("filterForm").addEventListener("submit", function (e) {
        e.preventDefault();
        loadPage(1);
    });

    loadPage(1); // Initial load
});
