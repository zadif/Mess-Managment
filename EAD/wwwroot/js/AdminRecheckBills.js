    let currentRequestRow = null;

    function openSweetAlertRecheck(requestId, userName, currentAmount, userMessage, btn) {
        currentRequestRow = btn.closest("tr");

    Swal.fire({
        title: 'Review Request',
    background: '#1e1e2d',
    color: '#fff',
    html: `
    <div style="text-align: left; font-size: 0.95rem; line-height: 1.6;">
        <p><strong>User:</strong> <span style="color: #fd5d38;">${userName}</span></p>
        <p><strong>Current Bill:</strong> <span class="text-success fw-bold">Rs. ${currentAmount}</span></p>
        <div style="background: #151521; padding: 10px; border-radius: 8px; margin: 10px 0; border: 1px solid #2b2b40;">
            <small style="color: #a1a5b7;">User says:</small><br>
                <em>"${userMessage}"</em>
        </div>
        <label class="mt-3 mb-1 d-block" style="font-size:0.85rem;">New Amount (Optional):</label>
        <input type="number" id="swal-new-amount" class="swal2-input" placeholder="Leave blank to keep Rs. ${currentAmount}" style="margin: 0; width: 100%;">
    </div>
    `,
    showDenyButton: true,
    showCancelButton: true,
    confirmButtonText: '<i class="fas fa-check"></i> Approve & Update',
    denyButtonText: '<i class="fas fa-times"></i> Reject Request',
    cancelButtonText: 'Cancel',
    confirmButtonColor: '#198754',
    denyButtonColor: '#dc3545',
    cancelButtonColor: '#6c757d',
    focusConfirm: false,
            preConfirm: () => {
                const inputVal = document.getElementById('swal-new-amount').value;
    if (inputVal) {
                    const newAmount = parseFloat(inputVal);
    if (isNaN(newAmount)) {
        Swal.showValidationMessage('Please enter a valid number');
    return false; 
                    }
    if (newAmount == 0) {
        Swal.showValidationMessage('Shabash, Full discount');
    return false;
        }
    if (newAmount < 0) {
        Swal.showValidationMessage('Bill amount cannot be negative!');
    return false;
                    }
                    if (newAmount > currentAmount) {
        Swal.showValidationMessage(`You cannot increase the bill! Max allowed is Rs. ${currentAmount}`);
    return false;
                    }
    return processRecheck(requestId, 'approve', newAmount);
                }
    return processRecheck(requestId, 'approve', null);
            },
            preDeny: () => {
                return processRecheck(requestId, 'reject', null);
            }
        });
    }

    function processRecheck(requestId, action, newAmount) {
        const body = "requestId=" + encodeURIComponent(requestId) + "&action=" + encodeURIComponent(action) + "&newAmount=" + encodeURIComponent(newAmount || "");
    return fetch("/Admin/ResolveRecheckRequest", {
        method: "POST",
    headers: {"Content-Type": "application/x-www-form-urlencoded" },
    body: body
        })
        .then(response => { if (!response.ok) throw new Error(response.statusText); return response.json(); })
        .then(result => {
            if (result && result.success) {
        updateTableRow(action, newAmount);
    Swal.fire({
        icon: 'success',
    title: 'Success!',
    text: action === 'approve' ? 'Bill updated successfully.' : 'Request rejected.',
    background: '#1e1e2d',
    color: '#fff',
    timer: 2000,
    showConfirmButton: false
                });
    return true;
            } else {
        Swal.showValidationMessage(`Request failed: ${result.message || 'Unknown error'}`);
    return false;
            }
        })
        .catch(error => {Swal.showValidationMessage(`Request failed: ${error}`); return false; });
    }

    function updateTableRow(action, newAmount) {
        if (!currentRequestRow) return;
    const statusCell = currentRequestRow.querySelector(".status-cell");
    const actionCell = currentRequestRow.querySelector(".action-cell");
    const amountCell = currentRequestRow.querySelector(".amount-cell");

    if (statusCell) {
        let badgeClass = action === "approve" ? "badge-approved" : (action === "reject" ? "badge-rejected" : "badge-processed");
    let statusText = action === "approve" ? "Approved" : (action === "reject" ? "Rejected" : "Processed");
    statusCell.innerHTML = `<span class="badge-custom ${badgeClass}">${statusText}</span>`;
        }

    if (actionCell) {
        actionCell.innerHTML = '<span class="text-muted small">Processed</span>';
        }

    const amountValue = parseFloat(newAmount);
        if (action === "approve" && !isNaN(amountValue) && amountValue > 0 && amountCell) {
        amountCell.textContent = "Rs. " + amountValue;
    amountCell.style.color = "#fd5d38"; 
            setTimeout(() => {amountCell.style.color = ""; amountCell.classList.add('text-success'); }, 2000);
        }
    }
