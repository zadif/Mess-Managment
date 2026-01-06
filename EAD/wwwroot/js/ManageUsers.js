document.addEventListener("DOMContentLoaded", () => {
  loadUsers();

  // Form Submit Handler
  document.getElementById("userForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    await saveUser();
  });
});

let allUsers = [];

async function loadUsers() {
  try {
    const response = await fetch("/Admin/GetUsersData");
    const result = await response.json();

    if (result.success) {
      allUsers = result.data;
      renderTable(allUsers);
    } else {
      Swal.fire("Error", result.message, "error");
    }
  } catch (error) {
    console.error("Error loading users:", error);
    Swal.fire("Error", "Failed to load users.", "error");
  }
}

function renderTable(users) {
  const tbody = document.getElementById("usersTableBody");
  const emptyState = document.getElementById("emptyState");
  const table = document.getElementById("usersTable");

  tbody.innerHTML = "";

  if (users.length === 0) {
    table.style.display = "none";
    emptyState.style.display = "block";
    return;
  }

  table.style.display = "table";
  emptyState.style.display = "none";

  users.forEach((user) => {
    const tr = document.createElement("tr");
    tr.innerHTML = `
            <td>#${user.id}</td>
            <td style="font-weight: 600;">${user.name}</td>
            <td style="color: var(--text-muted);">${user.email}</td>
            <td><span class="badge ${
              user.userType === "Admin" ? "badge-admin" : "badge-user"
            }">${user.userType}</span></td>
            <td><span class="badge ${
              user.isActive ? "badge-active" : "badge-inactive"
            }">${user.isActive ? "Active" : "Inactive"}</span></td>
            <td>
                <div class="action-buttons">
                    <button onclick="editUser(${
                      user.id
                    })" class="btn-action btn-edit">
                        <i class="fas fa-pen"></i> Edit
                    </button>
                    <button onclick="deleteUser(${
                      user.id
                    })" class="btn-action btn-delete">
                        <i class="fas fa-trash"></i> Delete
                    </button>
                </div>
            </td>
        `;
    tbody.appendChild(tr);
  });
}

function openModal() {
  document.getElementById("userForm").reset();
  document.getElementById("userId").value = "0";
  document.getElementById("modalTitle").textContent = "Add New User";
  document.getElementById("passwordHelp").textContent =
    "Required for new users";
  document.getElementById("userModal").style.display = "flex";
}

function closeModal() {
  document.getElementById("userModal").style.display = "none";
}

function editUser(id) {
  const user = allUsers.find((u) => u.id === id);
  if (!user) return;

  document.getElementById("userId").value = user.id;
  document.getElementById("userName").value = user.name;
  document.getElementById("userEmail").value = user.email;
  document.getElementById("userRole").value = user.userType;
  document.getElementById("userStatus").value = user.isActive
    .toString()
    .toLowerCase(); // Ensure string match
  document.getElementById("userPassword").value = ""; // Don't show password

  document.getElementById("modalTitle").textContent = "Edit User";
  document.getElementById("passwordHelp").textContent =
    "Leave blank to keep current password";

  document.getElementById("userModal").style.display = "flex";
}

async function saveUser() {
  const id = document.getElementById("userId").value;
  const name = document.getElementById("userName").value;
  const email = document.getElementById("userEmail").value;
  const password = document.getElementById("userPassword").value;
  const role = document.getElementById("userRole").value;
  const isActive = document.getElementById("userStatus").value === "true";

  const user = {
    Id: parseInt(id),
    Name: name,
    Email: email,
    Password: password,
    UserType: role,
    IsActive: isActive,
  };

  try {
    const response = await fetch("/Admin/SaveUserJson", {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(user),
    });

    const result = await response.json();

    if (result.success) {
      Swal.fire("Success", result.message, "success");
      closeModal();
      loadUsers();
    } else {
      Swal.fire("Error", result.message, "error");
    }
  } catch (error) {
    console.error("Error saving user:", error);
    Swal.fire("Error", "Failed to save user.", "error");
  }
}

async function deleteUser(id) {
  const result = await Swal.fire({
    title: "Are you sure?",
    text: "You won't be able to revert this!",
    icon: "warning",
    showCancelButton: true,
    confirmButtonColor: "#d33",
    cancelButtonColor: "#3085d6",
    confirmButtonText: "Yes, delete it!",
  });

  if (result.isConfirmed) {
    try {
      const response = await fetch(`/Admin/DeleteUserJson?id=${id}`, {
        method: "POST",
      });
      const data = await response.json();

      if (data.success) {
        Swal.fire("Deleted!", data.message, "success");
        loadUsers();
      } else {
        Swal.fire("Error", data.message, "error");
      }
    } catch (error) {
      console.error("Error deleting user:", error);
      Swal.fire("Error", "Failed to delete user.", "error");
    }
  }
}

// Close modal if clicked outside
window.onclick = function (event) {
  const modal = document.getElementById("userModal");
  if (event.target == modal) {
    closeModal();
  }
};
