// https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client?view=aspnetcore-3.1
"use strict";
(function () {
  const $connect = document.getElementById("connect");
  const $messages = document.getElementById("messages");
  console.log(window.location.origin + "/messaging");
  var recordNumber = 0;
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(window.location.origin + "/messaging")
    .configureLogging(signalR.LogLevel.Information)
    .build();

  $connect.onclick = function () {
    appendMessage("Connecting to Messaging Hub...");
    connection
      .start()
      .then(() => {
        console.log("Now connected");
        appendMessage("Connected.", "primary");
      })
      .catch((err) => appendMessage(err, "danger"));
  };

  connection.on("user_leader_board_info_added", (operation) => {
    appendMessage("User Leader Board Info Added", "success", operation);
  });

  function appendMessage(message, type, data) {
    var dataInfo = "";
    if (data) {
      dataInfo += "<div>Request Number Is: " + (++recordNumber) + "</div>";
      dataInfo += "<div>" + JSON.stringify(data) + "</div>";
    }
    $messages.innerHTML += `<li class="list-group-item list-group-item-${type}">${message} ${dataInfo}</li>`;
  }
})();
