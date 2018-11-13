﻿var nickName = "";
var nickTime = "";

$(function () {
    var vsPage = 7;//可见页数
    var pagesCount = 10;//总页数
    var curPage = 1;//当前页
    GetLeaveList(1, false);

});

function GetLeaveList(curPage, flag) {
    //var nickName = $("#searchNames").val();
    //var nickTime = $("#searchTimes").val();
    var roomID = $("#room").val();
    if (roomID == undefined)
        var roomID = "";
    else
        roomID = $("#room").val();
    if (roomID == "--请选择实验室--")
        roomID = "";
   // var pageSize = 2;  //每页多少条数据
    $.ajax({
        url: "/Leave/GetLeaveList",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: "{nickName:'" + nickName + "',nickTime:'" + nickTime + "',curPage:'" + curPage + "',roomID:'" + roomID + "'}",// "',pageSize:'" + pageSize +
        async: true,
        cache: false,
        type: "POST",
        success: function (data) {
            var obj = JSON.parse(data);
            var pageSize = obj.pageSize;
            if (obj.recordCount > pageSize) {
                ShowPage(obj.recordCount, pageSize, "pagesSums", curPage, flag);
            }
            var html = '';
            var index = (curPage - 1) * pageSize + 1;
            for (var i = 0 ; i < obj.lists.length ; i++) {
                html += '<tr><td>' + obj.lists[i].ID + '</td><td>' + obj.lists[i].Real_Name + '</td><td title="' + obj.lists[i].Reason + '">' + obj.lists[i].Reason + '</td><td title="' + obj.lists[i].Start_Time + '-' + obj.lists[i].End_Time + '">' + obj.lists[i].Start_Time + '-' + obj.lists[i].End_Time + '</td>';
                if (obj.lists[i].IsExamine == 0) {
                    if (roleCode == "R001" || roleCode == "R002") {
                        html += '<td><label class="btn btn-warning btn_more " type="button" onclick="Verfiy(' + obj.lists[i].ID + ','+ curPage+ ')">待审核</label>';
                    }
                   
                    else {
                       html += '<td><label class="btn btn-warning btn_more" type="button">待审核</label>';
                    }
                }
                else if (obj.lists[i].Pass == 0) {
                    html += '<td><label class="btn btn-danger btn_more" href="javascript:void(0);">未通过</label>';
                }
                else {
                    html += '<td><label class="btn btn-info btn_more" href="javascript:void(0);">同意</label>';
                }
            }
            if (obj.lists.length == 0) {
                html = '<p>暂无相关数据</p>' + html;
            }
            $("#sumItems").empty().html(html);
            $("#btn_Ask").unbind();
            $("#searchBtn").on("click", function (event) {
                    $('#pagesSums').empty();
                    $('#pagesSums').removeData("twbs-pagination");
                    $('#pagesSums').unbind("page");
                    nickTime = $("#searchTimes").val();
                    nickName = $("#searchNames").val();
                    GetLeaveList(1, false);
            });
            //请假申请
            $("#btn_Ask").on("click", function () {
                GetUserMessage();
                openAddLeave();
            });
        }
    });
}
function ShowPage(count, pageSize, id, curPage, flag) {
    pagesCount = Math.floor(count / pageSize) + (count % pageSize == 0 ? 0 : 1);
    vsPage = 7;
    vsPage = pagesCount > vsPage ? vsPage : pagesCount;
    $('#' + id).twbsPagination({
        totalPages: pagesCount,
        visiblePages: vsPage,
        startPage: curPage,
        version: '1.1',
        onPageClick: function (event, page) { //点击分页回调函数
            $('#' + id).find(".page").removeClass("active");
            $('#' + id).find(".page[data-page='" + page + "']").addClass("active");
            if (flag != false || flag == undefined) {
                GetLeaveList(page, true);
            }
            else {
                flag = true;
            }
        }
    });
}
function Verfiy(id,curpage) {
    var vsPage = 7;//可见页数
    var pagesCount = 10;//总页数
    
    layer.confirm('此申请是否通过？', {
        title: '请假申请', //标题
        btn: ['YES', 'NO'] //按钮
    }, function (index) {
        layer.close(index);//关闭弹窗
        //通过
        $.ajax({
            url: "/Leave/Update",
            contentType: "application/json; charset=utf-8",
            data: "{ID:'" + id + "','Pass':'" + 1 + "','IsExamine':'" + 1 + "'}",
            async: true,
            cache: false,
            type: "POST",
            success: function (data) {
                GetLeaveList(curpage, false);
            }
        });
    }, function () {
        //不通过
        $.ajax({
            url: "/Leave/Update",
            contentType: "application/json; charset=utf-8",
            data: "{ID:'" + id + "','Pass':'" + 0 + "','IsExamine':'" + 1 + "'}",
            async: true,
            cache: false,
            type: "POST",
            success: function (data) {
                GetLeaveList(curpage, false);
            }
        });
    });

}


function openAddLeave() {
    var UserID = $("#UserID").val();
    //iframe层
    layer.open({
        type: 2,
        title: '请假申请表',
        shadeClose: true,
        shade: 0.8,
        area: ['650px', '470px'],
        content: ['/Leave/AskLeave?id=' + UserID + "&flag=false", 'no'] //iframe的url
    });
}
function GetUserMessage()
{
    $.ajax({
        url: "/Leave/GetValid",
        contentType: "application/json; charset=utf-8",
        async: true,
        cache: false,
        type: "Get",
        success: function (data) {
        }
    });
}