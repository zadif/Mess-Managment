        // Confirm Payment Logic
        function confirmPayment(billId) {
            Swal.fire({
                title: 'Pay Bill?',
                text: "Are you sure you want to mark this bill as paid?",
                icon: 'question',
                showCancelButton: true,
                confirmButtonColor: '#4318FF',
                cancelButtonColor: '#d33',
                confirmButtonText: 'Yes, Mark Paid'
            }).then((result) => {
                if (result.isConfirmed) {
                    processPayment(billId);
                }
            });
        }

        function processPayment(billId) {
            Swal.fire({
                title: 'Processing...',
                didOpen: () => { Swal.showLoading() }
            });

            fetch("/UserMenu/MarkAsPaid", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: "billId=" + billId
            })
            .then(r => r.json())
            .then(result => {
                if (result === 1) {
                    Swal.fire('Success', 'Bill marked as paid!', 'success');
                    updateUiToPaid(billId);
                } else {
                    Swal.fire('Error', 'Could not update bill.', 'error');
                }
            });
        }


        function sendRecheckRequest(billId) {
            const msg = document.getElementById("recheckMsg").value.trim();
            if (!msg) {
                alert("Please write a reason");
                return;
            }

            fetch("/UserMenu/RequestRecheck", {
                method: "POST",
                headers: { "Content-Type": "application/x-www-form-urlencoded" },
                body: "billId=" + billId + "&msg=" + encodeURIComponent(msg)
            })
            .then(r => r.json())
            .then(result => {
                if (result === 1) {
                    closeConsumptionModal();
                    alert("Recheck request sent!");
                    location.reload();
                } else {
                    alert("Error");
                }
            });
        }

        function updateUiToPaid(billId) {
            const badge = document.getElementById(`badge-${billId}`);
            if (badge) {
                badge.className = "status-badge status-pending";
                badge.innerText = "Pending";
            }
            const actions = document.getElementById(`actions-${billId}`);
            if (actions) {
                actions.innerHTML = `<div class="w-100 text-center text-muted fw-bold p-2 bg-light rounded">
                    <i class="bi bi-hourglass-split"></i> Awaiting Verification
                </div>`;
            }
        }

        // Close modal on outside click
        window.onclick = function(e) {
            if (e.target.id === "consumptionModal") closeConsumptionModal();
        }



    function openRecheckPopup(billId) {
        fetch("/UserMenu/RecheckConsumptionsByBill", {
            method: "POST",
            headers: { "Content-Type": "application/x-www-form-urlencoded" },
            body: "billId=" + billId
        })
            .then(r => r.json())
            .then(consumptions => {
                let table = `
            <table class="table table-sm table-bordered">
                <thead class="table-light">
                    <tr>
                        <th>Date</th>
                        <th>Meal Item</th>
                        <th>Qty</th>
                        <th>Price</th>
                      
                        <th>Action</th>
                    </tr>
                </thead>
                <tbody>`;

                consumptions.forEach(c => {
                    const actionBtn = c.wasUserPresent
                        ? `<button type="button" class="btn btn-sm btn-outline-danger" onclick="sendSingleRecheck(${c.id}, this)">Send Recheck Request</button>`
                        : `<span class="text-muted">Request Sent</span>`;

                    table += `
                <tr>
                    <td>${new Date(c.consumptionDate).toLocaleDateString()}</td>
                    <td>${c.mealItem.name}</td>
                    <td>${c.quantity}</td>
                    <td>Rs. ${c.mealItem.price}</td>
               
                    <td>${actionBtn}</td>
                </tr>`;
                });

                table += `</tbody></table>`;
                document.getElementById("consumptionTable").innerHTML = table;
                document.getElementById("consumptionModal").style.display = "flex";
            });
    }

function closeConsumptionModal() {
    document.getElementById("consumptionModal").style.display = "none";
}

function sendSingleRecheck(consumptionId, button) {
    if (!confirm("Send recheck request for this item?")) return;

    button.disabled = true;
    button.innerHTML = "Sending...";

    fetch("/UserMenu/RecheckConsumptions", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: "id=" + consumptionId
    })
        .then(r => r.json())
        .then(result => {
            if (result === 1) {
                button.closest("td").innerHTML = '<span class="text-muted">Request Sent</span>';
                alert("Recheck request sent!");
            } else {
                button.disabled = false;
                button.innerHTML = "Send Recheck Request";
                alert("Error");
            }
        });
}

// Close modal on outside click
window.onclick = function (e) {
    if (e.target.id === "consumptionModal") closeConsumptionModal();
}
