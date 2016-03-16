
    <script src="~/assets/js/date-time/moment.js"></script>
    <script src="~/Scripts/mgm/services/mgm.services.employers.js"></script>
    <script type="text/javascript">


        mgm.page.startUp = function () {
            $(".fa-pencil").on("click", mgm.page.handlers.editThisEntry)
            $('.xButton').on('click', mgm.page.handlers.onDeleteClick);
            $('.delete').on('click', mgm.page.handlers.onDeleteConfirm);

            mgm.services.employers.getAll(mgm.page.onGetAllSuccess, mgm.page.onGetAllError)

            console.log("startup fired");
        };

        mgm.page.handlers.onDeleteClick = function (evt) {
            console.log("x button working");
            evt.preventDefault();

            mgm.page.theLastDeleteClick = this;

            $('#myModal').modal('show');
        };

        mgm.page.onGetAllSuccess = function (data, status, xhr) {          //getting list of objects
            //console.log(JSON.stringify(data));
            var employerList = data.items;                         

            for (var i = 0; i < employerList.length; i++) {

                var employer = {}
                var list = employerList[i];

                employer.id = list.id;
                employer.name = list.name;
                employer.email = list.email;
                employer.url = list.companyUrl;
                employer.jobTitle = list.jobTitle;
                employer.employmentType = list.employmentType;
                employer.isCurrent = list.isCurrent;
                employer.startDate = list.startDate;
                employer.endDate = list.endDate;

                mgm.page.appendToDom(employer)

            }
        };

        mgm.page.onGetAllError = function (jqXHR, textStatus) {
            console.log(JSON.stringify(jqXHR));
            var msgWindow = $("#msgWindow");

            msgWindow.addClass("alert-danger");
            msgWindow.removeAttr('style');
        };

        mgm.page.appendToDom = function (employer, target) {
            //console.log(employer);
            var clone = mgm.page.getClone();                              //Gets clone template

            if (employer.email != null) {
                clone.find('.email').text(employer.email);                  //If not null, enter email
            };
            if (employer.url != null) {
                clone.find('.url').text(employer.url);          //If not null, enter url val
                $('.url', clone).attr("href", employer.url);
            };

            var startDate = mgm.page.getDateFormat(employer.startDate)      // Change server date format
            var endDate = mgm.page.getDateFormat(employer.endDate)     // to readable format: 01/30/2016

            var employType = mgm.page.getEmploymentType(employer.employmentType); //Fulltime,part-time, etc
            var ifCurrent = mgm.page.getEndDateStatus(employer.isCurrent, endDate);  //If not current, show date.

            clone.attr("id", employer.id);
            clone.find('.name').html(employer.name);
            clone.find('.jobTitle').html(employer.jobTitle);
            clone.find('.employmentType').text(employType);
            clone.find('.startDate').text(startDate);
            clone.find('.endDate').html(ifCurrent);
            $('.editLink', clone).attr("href", employer.id + "/edit");

            var targetLoc = $('.col-xs-12') || target;

            targetLoc.append(clone);

            mgm.page.wireDeleteLink(clone);
        };            //input data into clone

        mgm.page.getDateFormat = function (date) {                    //get correct date format from server
            return moment(date).format("MM/DD/YYYY");
        };                      

        mgm.page.getEndDateStatus = function (current, endStatus) {   //If job is not current, show last date employed.
                                                                        // If job is current. Show "Present"
            if (!current == true) {                         
                current = endStatus;

            } else {
                current = "Present";                         
            }
            return current;
        };     

        mgm.page.getEmploymentType = function (type) {              //convert int16 to string (full-time , part-time etc)

            if (type == 0 || null) {
                type = "0";
            } else if (type == 1) {
                type = "Full-Time";
            } else if (type == 2) {
                type = "Part-Time";
            } else if (type == 3) {
                type = "Seasonal";
            } else if (type == 4) {
                type = "Other";
            }
            return type;
        };                  

        mgm.page.getClone = function () {                         //clone HTML template
            var cloneHTML = $("#employer-template").html();
            var clone = $(cloneHTML).clone();

            return clone;
        };                               

        mgm.objects = [];

        mgm.page.wireDeleteLink = function (context, data) {      //Wires the delete button

            var deleteLink = $(".xButton", context);

            mgm.objects.push(deleteLink);
            deleteLink.on('click', mgm.page.handlers.onDeleteClick)
        };

        mgm.page.handlers.onDeleteConfirm = function () {
            var currentDiv = $(mgm.page.theLastDeleteClick).closest('.widget-box');
            var id = currentDiv.attr('id');

            mgm.services.employers.delete(id,
                mgm.page.onDeleteSuccess, mgm.page.onDeleteError);

            $('#myModal').modal('hide');                                     //Deletes employer on confirm
        };                                                                   //On modal delete click, Deletes element

        mgm.page.onDeleteSuccess = function () {                           //Delete Object from Dom
            var currentDiv = $(mgm.page.theLastDeleteClick).closest('.widget-box');
            var speed = 1200;

            currentDiv.fadeOut(speed, mgm.page.removeCurrentObject);
        };

        mgm.page.removeCurrentObject = function () {
            $(this).remove();
        };

        mgm.page.onDeleteError = function (data, status, xhr) {
            console.log(JSON.stringify(data));
            console.log(JSON.stringify(xhr));
        };




    </script>


    <script type="text/template" id="employer-template">

        <div class="widget-box col-sm-4">
            <div class="widget-header">
                <h4 class="widget-title"><strong class="name">Lakers</strong></h4>
                <!-- #section:custom/widget-box.toolbar -->
                <div class="widget-toolbar">
                    <div class="widget-menu">
                        <a class="editLink" href="#">
                            <i class="ace-icon fa fa-pencil bigger-125"></i>
                        </a>
                        <a href="#" class="xButton">
                            <i class="ace-icon fa fa-times"></i>
                        </a>
                    </div>
                </div>
                <!-- /section:custom/widget-box.toolbar -->
            </div>
            <div class="widget-body" style="display: block">
                <div class="widget-main">
                    <div class="alert-shade center">
                        <div>
                            <img class="get-imgsize" src="http://www.obitsforlife.com/images/defaultLogo.png" />
                            <div class="email-url">
                                <a class="url"> The URL was not provided.</a>
                                <br />
                                <strong><a class="email">The email was not provided.</a></strong>
                            </div>
                        </div>
                        <br />

                    </div>
                    <div class="alert alert-info">
                        <center>
                            <span><strong class="employmentType">Full-Time</strong></span>
                            <br />
                            <span><strong class="jobTitle">Lakers Coach</strong></span>

                        </center>
                    </div>
                    <div class="alert alert-success">
                        <center>
                            <label><strong>Time Employed: </strong></label>
                            <br />
                            <span><strong class="startDate">00/00/0000</strong></span> - <span><strong class="endDate">00/00/0000</strong></span>
                        </center>
                    </div>
                </div>
            </div>
        </div>
    </script>

    <div id="myModal" class="modal fade" role="dialog">
        <div class="modal-dialog">

            <!-- Modal content-->
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">Warning</h4>
                </div>
                <div class="modal-body">
                    <p><strong>Are you sure you would like to delete this employer?</strong></p>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-danger delete">Delete</button>
                    <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                </div>
            </div>

        </div>
    </div>
