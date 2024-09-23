var dataTable
$(document).ready(function () { loadDataTable(); })
function loadDataTable()
{
    dataTable = $('#tblData').DataTable
        ({
            "ajax": "/Admin/CoverType/GetAll",
            "columns": [{ "data": "name", "width": "80%" },
                {
                    "data": "id", "render": function (data)
                    {
                        return `

                        <div class="text-center">
                        <a href="/Admin/CoverType/Upsert/${data}" class="btn btn-info">
                        <i class="fas fa-edit"></i>
                        </a>
                        <a class="btn btn-danger"onclick=Delete("/Admin/CoverType/Delete/${data}")>
                        <i class="fas fa-trash"></i>
                        </a>
                        </div>
                        `;
                    }
                }
            ]
        })
}
function Delete(url)
{
    swal({
        icon: "warning",
        text: "Once deleted, you will not be able to recover",
        title: "Are you sure ?",
        buttons: true,
        dangerModel:true
    }).then((willdelete) => {
        if (willdelete) {
            $.ajax({
                url: url,
                type: "DELETE",
                
                success: function (data)
                {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message)
                    }
                    else
                    {
                        toastr.error(data.message)
                    }
                }
            })
        }
    })
}