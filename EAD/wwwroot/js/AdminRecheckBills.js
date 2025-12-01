
    let currentRequestId = 0;
    let currentRequestRow = null;

    function openRecheckModal(requestId, userName, currentAmount, userMessage, btn) {
        currentRequestId = requestId;
    currentRequestRow = btn ? btn.closest("tr") : document.querySelector(`tr[data-request-id="${requestId}"]`);

    document.getElementById("modalUserName").textContent = userName;
    document.getElementById("modalCurrentAmount").textContent = "Rs. " + currentAmount;
    document.getElementById("modalUserMessage").textContent = userMessage;
    document.getElementById("newAmount").value = "";
    document.getElementById("recheckModal").style.display = "flex";
    }

    function closeRecheckModal() {
        document.getElementById("recheckModal").style.display = "none";
    }

    function resolveRequest(action, button) {
        const newAmountInput = document.getElementById("newAmount");
    const newAmount = newAmountInput.value;

    // disable all resolve buttons in the modal
    const modalButtons = document.querySelectorAll(
    '#recheckModal button[onclick^="resolveRequest"]'
    );
        modalButtons.forEach(btn => {
        btn.disabled = true;
    if (!btn.dataset.originalText) {
        btn.dataset.originalText = btn.innerHTML;
            }
        });

    // show "Processing..." on the clicked button
    if (button) {
        button.innerHTML = "Processing...";
        }

    const body =
    "requestId=" + encodeURIComponent(currentRequestId) +
    "&action=" + encodeURIComponent(action) +
    "&newAmount=" + encodeURIComponent(newAmount);

    fetch("/Admin/ResolveRecheckRequest", {
        method: "POST",
    headers: {"Content-Type": "application/x-www-form-urlencoded" },
    body: body
        })
            .then(r => r.json())
            .then(result => {
                if (result && result.success) {
                    // ✅ Update the row in the table instead of reloading
                    if (currentRequestRow) {
                        const statusCell = currentRequestRow.querySelector(".status-cell");
    const actionCell = currentRequestRow.querySelector(".action-cell");
    const amountCell = currentRequestRow.querySelector(".amount-cell");

    let newStatusText;
    if (action === "approve") {
        newStatusText = "Approved";
                        } else if (action === "reject") {
        newStatusText = "Rejected";
                        } else {
        newStatusText = "Resolved";
                        }

    if (statusCell) {
        statusCell.innerHTML = `<span class="badge bg-secondary">${newStatusText}</span>`;
                        }

    if (actionCell) {
        actionCell.innerHTML = '<span class="text-success">Processed</span>';
                        }

    const amountValue = parseFloat(newAmount);
                        if (action === "approve" && !isNaN(amountValue) && amountValue > 0 && amountCell) {
        amountCell.textContent = "Rs. " + amountValue;
                        }
                    }

    closeRecheckModal();
                    // no page reload
                } else {
        // re-enable buttons and restore text on error
        modalButtons.forEach(btn => {
            btn.disabled = false;
            if (btn.dataset.originalText) {
                btn.innerHTML = btn.dataset.originalText;
            }
        });
    alert("Error processing request");
                }
            })
            .catch(err => {
        console.error("resolveRequest error:", err);
                // re-enable buttons and restore text on network error
                modalButtons.forEach(btn => {
        btn.disabled = false;
    if (btn.dataset.originalText) {
        btn.innerHTML = btn.dataset.originalText;
                    }
                });
    alert("Network error");
            });
    }

    window.onclick = e => {
        if (e.target.id === "recheckModal") closeRecheckModal();
    };
