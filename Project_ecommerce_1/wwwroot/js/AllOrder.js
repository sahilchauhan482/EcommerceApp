$(document).ready(function () {


    // Initialize DataTable
    var table = $('#orderTable').DataTable({
        "ajax": {
            "url": "/Admin/OrderManagement/GetAllOrders",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "orderId", "autoWidth": true },
            { "data": "status", "autoWidth": true },
            {
                "data": "orderDate",
                "autoWidth": true,
                "render": function (data) {
                    var dateObject = new Date(data);
                    var options = { day: '2-digit', month: '2-digit', year: 'numeric' };
                    var formattedDate = dateObject.toLocaleDateString(undefined, options);
                    return formattedDate;
                }
            },
            { "data": "customerName", "autoWidth": true },
            { "data": "phoneNumber", "autoWidth": true },
            { "data": "customerEmail", "autoWidth": true },
            {
                "data": "orderId", "render": function (data) {
                    return "<a href='/Admin/OrderManagement/Details/" + data + "' class='btn btn-info btn-sm'><i class='fa fa-eye'></i> View Details</a>";
                },
                "autoWidth": true
            },
        ],
        "initComplete": function () {
            var uniqueStatus = Array.from(new Set(table.column(1).data().toArray()));
            var select = $('<select class="status-dropdown form-control"><option value="">Select Status</option><option value="All">All</option></select>')
                .appendTo($('#orderTable_wrapper .dataTables_filter'))
                .on('change', function () {
                    var selectedValue = $(this).val();
                    if (selectedValue === "All") {
                        table.column(1).search("").draw();
                    } else {
                        table.column(1).search(selectedValue).draw();
                    }
                });

            uniqueStatus.forEach(status => {
                select.append('<option value="' + status + '">' + status + '</option>');
            });

            select.val("").trigger("change");

            var wrapper = $('#orderTable_wrapper');
            var filter = wrapper.find('.dataTables_filter');
            select.appendTo(filter);

            $('.status-dropdown').select2({
                width: '150px'
            });

            
            $('#fromDate, #toDate').on('change', function () {
                var fromDate = $('#fromDate').val();
                var toDate = $('#toDate').val();

                table.draw();
            });
        }
    });

    
    $.fn.dataTable.ext.search.push(
        function (settings, data, dataIndex) {
            var fromDate = $('#fromDate').val();
            var toDate = $('#toDate').val();
            var orderDate = new Date(data[2]); 

            
            var fromDateObj = fromDate ? new Date(fromDate) : null;
            var toDateObj = toDate ? new Date(toDate) : null;

            
            return (!fromDateObj || orderDate >= fromDateObj.setHours(0, 0, 0, 0)) &&
                (!toDateObj || orderDate <= toDateObj.setHours(23, 59, 59, 999));
        }
    );

    
    $('#fromDate, #toDate').on('change', function () {
        table.draw();
    });
});