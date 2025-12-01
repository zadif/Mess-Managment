    let currentBillId = 0;

    // MARK AS PAID
    function markAsPaid(billId, button) {
    if (!confirm("Have you paid this bill?")) return;

    button.disabled = true;
    button.innerHTML = "Processing...";

    fetch("/UserMenu/MarkAsPaid", {
        method: "POST",
 
    body: new URLSearchParams({billId: billId })
    })
    .then(r => r.json())
    .then(result => {
        if (result === 1) {
            const card = button.closest(".bill-card");
    const header = card.querySelector(".card-header");
    const footer = card.querySelector(".card-footer");

    header.className = "card-header bg-warning text-dark";
    footer.innerHTML = '<div class="badge bg-warning text-dark fs-5 px-4 py-2">Paid - Awaiting Verification</div>';

            document.querySelector(".changer").innerHTML = "Paid - Awaiting Verification";
            document.querySelector(".changer")
                .classList.remove("bg-danger")
                .classList.add("bg-warning", "text-dark");

        } else {
        button.disabled = false;
    button.innerHTML = "Mark as Paid";
    alert("Error!");
        }
    })
    .catch(() => {
        button.disabled = false;
    button.innerHTML = "Mark as Paid";
    alert("Network error!");
    });
}

    // REQUEST RECHECK
    function requestRecheck(billId) {
        currentBillId = billId;
    document.getElementById("recheckMessage").value = "";
    document.getElementById("recheckModal").style.display = "flex";
}

    function closeRecheckModal() {
        document.getElementById("recheckModal").style.display = "none";
}

    function sendRecheckRequest() {
    const message = document.getElementById("recheckMessage").value.trim();
    if (!message) {
        alert("Please write a message");
    return;
    }

    const params = new URLSearchParams();
    params.append("billId", currentBillId);
    params.append("msg", message);

    fetch("/UserMenu/RequestRecheck", {
        method: "POST",
    body: params,
   
    })
    .then(r => r.json())
    .then(result => {
        if (result === 1) {
            closeRecheckModal();
            const card = button.closest(".bill-card");
            const header = card.querySelector(".card-header");
            const footer = card.querySelector(".card-footer");

            header.className = "card-header bg-warning text-dark";
            footer.innerHTML = '<div class="badge bg-warning text-dark fs-5 px-4 py-2"> Recheck Request send</div>';

            document.querySelector(".changer").innerHTML = "Sent for recheck";
          
            document.querySelectorAll(
                'button.btn-success.flex-fill[onclick^="markAsPaid"], ' +
                'button.btn-outline-danger.flex-fill[onclick^="requestRecheck"]'
            ).forEach(btn => {
                // add a class to both buttons
                btn.classList.add('hidden-bill-action');

                // hide them
                btn.style.display = 'none';

                // or, if you want them completely removed from the DOM:
                // btn.remove();
            });


        } else {
        alert("Error sending request");
        }
    })
    .catch(() => alert("Network error"));
}

    // Close modal when clicking outside
    window.onclick = function(e) {
    const modal = document.getElementById("recheckModal");
    if (e.target === modal) closeRecheckModal();
}
