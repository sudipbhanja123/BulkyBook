//var userDataTable;

//$(document).ready(function () {
//    loadDataTable();
//})
//function loadDataTable() {
//    userDataTable = $('#tblData').DataTable({
//        ajax: '/admin/user/getall',
//        columns: [
//            { data: 'name', width: '25%' },
//            { data: 'email', width: '15%' },
//            { data: 'phoneNumber', width: '10%'},

//            {
//                data: 'role', width: '10%', "render": function (data) {
//                    return '<span class="badge bg-secondary">' + data + '</span>';
//                }
//            },
//            {
//                data: 'id', width: '25%', "render": function (data) {
//                    var today = new Date().getTime();
//                    var lockout = new Date(data.lockoutEnd).getTime();
//                    var isLocked = lockout > today;
//                    return `<div class="d-flex gap-2 justify-content-end">
//                      <a onclick="LockUnlock('${data}')" class="btn btn-sm ${isLocked?'btn-danger':'btn-success'}">

//                                <i class="mx-1 bi bi-${isLocked ? 'lock' : 'unlock'}-fill"></i>${isLocked ? 'lock' : 'unlock'}
//                            </a>
//                            <a href = "/admin/user/RoleManagmengt?userId=${data}" class="btn btn-sm btn-outline-secondary">
//                                <i class="bi bi-person-badge"></i>Role
//                            </a>
//                             <a onclick = "Delete('/admin/user/ChangePassword?userId=${data}')" class="btn btn-sm btn-outline-danger">
//                                <i class="bi bi-key-fill"></i>Password
//                            </a>
//                       </div>`;
//                }
//            }
//        ]
//    });

//    function LockUnlock(id) {
//        $.ajax({
//            type: "POST",
//            url: '/admin/user/LockUnlock',
//            data: JSON.stringify(id),
//            contentType: "application/json",
//            success: function (data) {
//                if (data.success) {
//                    toastr.success(data.message);
//                    userDataTable.ajax.reload();
//                }
//            }
//        });
//    }
//}


var userDataTable;

$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {

    userDataTable = $('#tblData').DataTable({
        ajax: '/admin/user/getall',

        columns: [
            {
                data: 'name',
                width: '25%'
            },
            {
                data: 'email',
                width: '15%'
            },
            {
                data: 'phoneNumber',
                width: '10%'
            },
            {
                data: 'role',
                width: '10%',
                render: function (data) {
                    return '<span class="badge bg-secondary">' + data + '</span>';
                }
            },
            {
                data: { id: 'id', lockoutEnd: 'lockoutEnd' },
                width: '25%',
                render: function (data, type, row) {

                    // data = user ID
                    // row = complete user object

                    var today = new Date().getTime();

                    var lockout = row.lockoutEnd
                        ? new Date(row.lockoutEnd).getTime()
                        : 0;

                    var isLocked = lockout > today;

                    return `
                        <div class="d-flex gap-2 justify-content-end">

                            <a href="javascript:void(0);"
                               onclick="LockUnlock('${data.id}')"
                               class="btn btn-sm ${isLocked ? 'btn-danger' : 'btn-success'}">

                                <i class="mx-1 bi bi-${isLocked ? 'lock-fill' : 'unlock-fill'}"></i>
                                ${isLocked ? 'lock' : 'unlock'}

                            </a>

                            <a href="/admin/user/RoleManagement?userId=${data.id}"
                               class="btn btn-sm btn-outline-secondary">

                                <i class="bi bi-person-badge"></i>
                                Role

                            </a>

                            <a href="/admin/user/ChangePassword?userId=${data.id}"
                               class="btn btn-sm btn-outline-danger">

                                <i class="bi bi-key-fill"></i>
                                Password

                            </a>

                        </div>
                    `;
                }
            }
        ]
    });
}


function LockUnlock(id) {

    console.log("LockUnlock called with ID:", id);

    $.ajax({
        type: "POST",
        url: "/admin/user/LockUnlock",
        data: JSON.stringify(id),
        contentType: "application/json; charset=utf-8",

        success: function (data) {

            if (data.success) {

                toastr.success(data.message);

                userDataTable.ajax.reload(null, false);

            } else {

                toastr.error(data.message);
            }
        },

        error: function (xhr, status, error) {

            console.error("LockUnlock API Error:");
            console.error("Status:", xhr.status);
            console.error("Response:", xhr.responseText);
            console.error("Error:", error);

            toastr.error("Something went wrong while locking/unlocking the user.");
        }
    });
}