        // 1. Confirm Payment Logic
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
        // Show loading spinner
        Swal.fire({
            title: 'Processing...',
            didOpen: () => { Swal.showLoading() }
        });

    // AJAX Call
    fetch("/UserMenu/MarkAsPaid", {
        method: "POST",
    headers: {"Content-Type": "application/x-www-form-urlencoded" },
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
            .catch(err => {
        console.error(err);
    Swal.fire('Error', 'Network error occurred.', 'error');
            });
        }

    // 2. Request Recheck Logic
    function openRecheckPopup(billId) {
        Swal.fire({
            title: 'Request Recheck',
            input: 'textarea',
            inputLabel: 'Why is the bill incorrect?',
            inputPlaceholder: 'Type your reason here...',
            showCancelButton: true,
            confirmButtonText: 'Send Request',
            confirmButtonColor: '#e31a1a',
            preConfirm: (message) => {
                if (!message) {
                    Swal.showValidationMessage('Please write a reason');
                }
                return message;
            }
        }).then((result) => {
            if (result.isConfirmed) {
                sendRecheck(billId, result.value);
            }
        });
        }

    function sendRecheck(billId, msg) {
        Swal.fire({
            title: 'Sending...',
            didOpen: () => { Swal.showLoading() }
        });

    const params = new URLSearchParams();
    params.append("billId", billId);
    params.append("msg", msg);

    fetch("/UserMenu/RequestRecheck", {
        method: "POST",
    body: params
            })
            .then(r => r.json())
            .then(result => {
                if (result === 1) {
        Swal.fire('Sent', 'Recheck request sent to admin.', 'success');
    updateUiToRecheck(billId);
                } else {
        Swal.fire('Error', 'Failed to send request.', 'error');
                }
            })
            .catch(err => {
        Swal.fire('Error', 'Network error.', 'error');
            });
        }

    // 3. UI Helper Functions (Update page without reload)
    function updateUiToPaid(billId) {
            // Update Badge
            const badge = document.getElementById(`badge-${billId}`);
    if(badge) {
        badge.className = "status-badge status-pending";
    badge.innerText = "Pending";
            }
    // Update Buttons
    const actions = document.getElementById(`actions-${billId}`);
    if(actions) {
        actions.innerHTML = `
                    <div class="w-100 text-center text-muted fw-bold p-2 bg-light rounded">
                        <i class="bi bi-hourglass-split"></i> Awaiting Verification
                    </div>`;
            }
        }

    function updateUiToRecheck(billId) {
            const badge = document.getElementById(`badge-${billId}`);
    if(badge) {
        badge.className = "status-badge status-pending";
    badge.innerText = "In Recheck";
            }
    const actions = document.getElementById(`actions-${billId}`);
    if(actions) {
        actions.innerHTML = `
                    <div class="w-100 text-center text-warning fw-bold p-2 bg-light rounded">
                        <i class="bi bi-envelope-paper"></i> Recheck Sent
                    </div>`;
            }
        }
