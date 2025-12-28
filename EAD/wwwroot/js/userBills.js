// --- GLOBAL VARIABLES ---
let pendingRecheckId = 0; // Stores the ID of the item to be rechecked
let confirmModalInstance = null; // Stores Bootstrap modal instance

// --- PAYMENT LOGIC (Existing) ---
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
        })
        .catch(err => Swal.fire('Error', 'Network error', 'error'));
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

// --- CONSUMPTION LIST MODAL LOGIC ---

function openRecheckPopup(billId) {
    // Show modal immediately with loader
    document.getElementById("consumptionModal").style.display = "flex";
    document.getElementById("consumptionTable").innerHTML = '<div class="text-center py-5"><div class="spinner-border text-primary" role="status"></div></div>';

    fetch("/UserMenu/RecheckConsumptionsByBill", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: "billId=" + billId
    })
        .then(r => r.json())
        .then(consumptions => {
            let table = `
        <table class="table table-hover align-middle">
            <thead class="table-light">
                <tr>
                    <th>Date</th>
                    <th>Meal Item</th>
                    <th>Qty</th>
                    <th>Price</th>
                    <th class="text-end">Action</th>
                </tr>
            </thead>
            <tbody>`;

            if (consumptions.length === 0) {
                table += `<tr><td colspan="5" class="text-center p-4">No items found.</td></tr>`;
            }

            consumptions.forEach(c => {
                // Logic to determine button state
                let actionHtml = '';

                // Note: We assign a specific ID to the TD (cell) so we can update it later via JS
                if (c.wasUserPresent) {
                    // If present and not rechecked yet, show button
                    actionHtml = `
                    <button type="button" class="btn btn-sm btn-outline-danger fw-bold" 
                            onclick="openConfirmRecheck(${c.id})">
                        Request Recheck
                    </button>`;
                } else {
                    // Already rechecked
                    actionHtml = `<span class="badge bg-secondary text-light">Request Sent</span>`;
                }

                table += `
            <tr>
                <td>${new Date(c.consumptionDate).toLocaleDateString()}</td>
                <td><span class="fw-bold text-dark">${c.mealItem.name}</span></td>
                <td>${c.quantity}</td>
                <td>Rs. ${c.mealItem.price}</td>
                <td class="text-end" id="action-cell-${c.id}">
                    ${actionHtml}
                </td>
            </tr>`;
            });

            table += `</tbody></table>`;
            document.getElementById("consumptionTable").innerHTML = table;
        })
        .catch(err => {
            document.getElementById("consumptionTable").innerHTML = '<p class="text-danger text-center">Failed to load data.</p>';
        });
}

function closeConsumptionModal() {
    document.getElementById("consumptionModal").style.display = "none";
}

// --- RECHECK CONFIRMATION POPUP LOGIC ---

function openConfirmRecheck(id) {
    pendingRecheckId = id; // Store ID

    const modalEl = document.getElementById('confirmRecheckModal');
    if (modalEl) {
        // Use Bootstrap API
        confirmModalInstance = new bootstrap.Modal(modalEl);
        confirmModalInstance.show();
    }
}

function closeConfirmRecheck() {
    if (confirmModalInstance) {
        confirmModalInstance.hide();
    }
}

function submitSingleRecheck() {
    // 1. Disable button in popup to prevent double click
    const confirmBtn = document.querySelector('#confirmRecheckModal .btn[onclick="submitSingleRecheck()"]');
    if (confirmBtn) {
        confirmBtn.disabled = true;
        confirmBtn.innerText = "Sending...";
    }

    // 2. API Call
    fetch("/UserMenu/RecheckConsumptions", {
        method: "POST",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body: "id=" + pendingRecheckId
    })
        .then(r => r.json())
        .then(result => {
            if (result === 1) {
                // 3. Update UI (The table row in the background modal)
                const cell = document.getElementById(`action-cell-${pendingRecheckId}`);
                if (cell) {
                    cell.innerHTML = `<span class="badge bg-secondary text-light">Request Sent</span>`;
                }

                // 4. Close confirmation modal
                closeConfirmRecheck();

                // 5. Success Toast
                Swal.fire({
                    icon: 'success',
                    title: 'Sent',
                    text: 'Recheck request submitted.',
                    toast: true,
                    position: 'top-end',
                    showConfirmButton: false,
                    timer: 3000
                });
            } else {
                Swal.fire('Error', 'Failed to send request.', 'error');
            }
        })
        .catch(err => {
            Swal.fire('Error', 'Network Error', 'error');
        })
        .finally(() => {
            // Reset button state
            if (confirmBtn) {
                confirmBtn.disabled = false;
                confirmBtn.innerText = "Yes, Recheck";
            }
        });
}

// Close custom modal on outside click (Consumption Modal)
window.onclick = function (e) {
    if (e.target.id === "consumptionModal") closeConsumptionModal();
}