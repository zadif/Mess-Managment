let currentBillId = 0;
let currentBillCard = null;

// MARK AS PAID
function markAsPaid(billId, button) {
    if (!confirm("Have you paid this bill?")) return;

    // get both buttons in this card and disable them
    const card = button.closest(".bill-card");
    const cardButtons = card.querySelectorAll(
        'button.btn-success.flex-fill[onclick^="markAsPaid"], ' +
        'button.btn-outline-danger.flex-fill[onclick^="requestRecheck"]'
    );
    cardButtons.forEach(b => b.disabled = true);
    button.innerHTML = "Processing...";

    fetch("/UserMenu/MarkAsPaid", {
        method: "POST",
        body: new URLSearchParams({ billId: billId })
    })
        .then(r => r.json())
        .then(result => {
            if (result === 1) {
                const header = card.querySelector(".card-header");
                const footer = card.querySelector(".card-footer");

                if (header) {
                    header.className = "card-header bg-warning text-dark";
                }
                if (footer) {
                    footer.innerHTML = '<div class="badge bg-warning text-dark fs-5 px-4 py-2">Paid - Awaiting Verification</div>';
                }

                const badge = card.querySelector(".changer");
                if (badge) {
                    badge.innerHTML = "Paid - Awaiting Verification";
                    badge.classList.remove("bg-danger");
                    badge.classList.add("bg-warning", "text-dark");
                }
            } else {
                cardButtons.forEach(b => b.disabled = false);
                button.innerHTML = "Mark as Paid";
                alert("Error!");
            }
        })
        .catch(err => {
            console.error("markAsPaid error:", err);
            cardButtons.forEach(b => b.disabled = false);
            button.innerHTML = "Mark as Paid";
            alert("Network error!");
        });
}

// REQUEST RECHECK
function requestRecheck(billId, button) {
    currentBillId = billId;
    currentBillCard = button.closest(".bill-card");

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

    const sendBtn = document.getElementById("recheckSendBtn");
    if (sendBtn) {
        sendBtn.disabled = true;
        if (!sendBtn.dataset.originalText) {
            sendBtn.dataset.originalText = sendBtn.innerHTML;
        }
        sendBtn.innerHTML = "Sending...";
    }

    const params = new URLSearchParams();
    params.append("billId", currentBillId);
    params.append("msg", message);

    fetch("/UserMenu/RequestRecheck", {
        method: "POST",
        body: params
    })
        .then(r => r.json())
        .then(result => {
            if (result === 1) {
                closeRecheckModal();

                if (currentBillCard) {
                    const header = currentBillCard.querySelector(".card-header");
                    const footer = currentBillCard.querySelector(".card-footer");
                    const badge = currentBillCard.querySelector(".changer");

                    if (header) {
                        header.className = "card-header bg-warning text-dark";
                    }
                    if (footer) {
                        footer.innerHTML = '<div class="badge bg-warning text-dark fs-5 px-4 py-2">Recheck Request sent</div>';
                    }
                    if (badge) {
                        badge.innerHTML = "Sent for recheck";
                    }

                    // hide only this card's action buttons
                    currentBillCard.querySelectorAll(
                        'button.btn-success.flex-fill[onclick^="markAsPaid"], ' +
                        'button.btn-outline-danger.flex-fill[onclick^="requestRecheck"]'
                    ).forEach(btn => {
                        btn.classList.add("hidden-bill-action");
                        btn.style.display = "none";
                    });
                }

                if (sendBtn) {
                    sendBtn.disabled = false;
                    sendBtn.innerHTML = sendBtn.dataset.originalText || "Send Request";
                }
            } else {
                if (sendBtn) {
                    sendBtn.disabled = false;
                    sendBtn.innerHTML = sendBtn.dataset.originalText || "Send Request";
                }
                alert("Error sending request");
            }
        })
        .catch(err => {
            console.error("sendRecheckRequest error:", err);
            if (sendBtn) {
                sendBtn.disabled = false;
                sendBtn.innerHTML = sendBtn.dataset.originalText || "Send Request";
            }
            alert("Network error");
        });
}

// Close modal when clicking outside
window.onclick = function (e) {
    const modal = document.getElementById("recheckModal");
    if (e.target === modal) closeRecheckModal();
};
