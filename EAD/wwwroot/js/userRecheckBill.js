
    function viewMessage(reqId, message) {
        Swal.fire({
            title: `<div style='font-size:1.2rem; color:#2d3748'>Request <span style='color:#5a67d8'>#${reqId}</span></div>`,
            html: `
                <div style="text-align: left; background: #f8fafc; padding: 20px; border-radius: 12px; border: 1px solid #e2e8f0;">
                    <div style="display:flex; align-items:center; margin-bottom:10px;">
                        <i class="fas fa-quote-left" style="color:#cbd5e0; margin-right:10px;"></i>
                        <span style="font-weight:bold; color:#718096; font-size:0.85rem; text-transform:uppercase;">User Message</span>
                    </div>
                    <p style="margin: 0; color: #4a5568; font-size: 1rem; line-height: 1.6;">${message}</p>
                </div>
            `,
            showCloseButton: true,
            focusConfirm: false,
            confirmButtonText: 'Okay, Got it',
            confirmButtonColor: '#5a67d8',
            buttonsStyling: true,
            customClass: {
                popup: 'rounded-4 shadow-lg',
                confirmButton: 'btn btn-theme px-4 py-2 rounded-pill'
            }
        });
    }
