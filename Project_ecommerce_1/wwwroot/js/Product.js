var dataTable;
$(document).ready(function () {
    loadDataTable();
})
function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/Admin/product/GetAll"
        },
        "columns":
            [
                { "data": "title", "width": "15%" },
                { "data": "description", "width": "20" },
                { "data": "author", "width": "15%" },
                { "data": "isbn", "width": "15%" },
                { "data": "price", "width": "15%" },
                {
                    "data": "id",
                    "render": function (data) {
                        return `
                    <div class="text-center">
                    <a href="/Admin/Product/Upsert/${data}" class="btn btn-info">
                    <i class="fas fa-edit"></i>
                    </a>
                    <a class="btn btn-danger" onclick=Delete("/Admin/Product/Delete/${data}")>
                    <i class="fas fa-trash"></i>
                    </a>
                    
                    </div>


                    `;
                    }
                }
            ]
    }

    )
}

function Delete(url) {
    swal({
        title: "Are you sure",
        text: "Once deleted , can not be recovered",
        icon: "warning",
        buttons: true,
        dangerModel: true


    }).then((willdelete) => {
        if (willdelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);

                    }
                    else {
                        toastr.error(data.message);
                    }

                }

            })
        }

    })
}