// SignalR Notification Client
let connection = null;
let isConnected = false;

// Khởi tạo SignalR connection
function initSignalR() {
    if (connection) {
        return; // Đã khởi tạo rồi
    }

    connection = new signalR.HubConnectionBuilder()
        .withUrl("/notificationHub")
        .withAutomaticReconnect()
        .build();

    // Kết nối thành công
    connection.onreconnecting(() => {
        console.log("SignalR: Đang kết nối lại...");
    });

    connection.onreconnected(() => {
        console.log("SignalR: Đã kết nối lại");
        joinUserGroups();
    });

    // Xử lý Order Updated
    connection.on("OrderUpdated", (data) => {
        console.log("OrderUpdated received:", data);
        showNotification("Đơn hàng #" + data.orderId + ": " + data.message, "info");
        // Reload page nếu đang ở trang order detail hoặc order list
        const path = window.location.pathname.toLowerCase();
        if (path.includes("/orders/detail") || 
            path.includes("/orders/index") ||
            path.includes("/buyer/orders") ||
            path.includes("/seller/orders")) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
    });

    // Xử lý New Order (cho seller)
    connection.on("NewOrder", (data) => {
        console.log("NewOrder received:", data);
        const orderText = data.orderId > 0 ? "#" + data.orderId : "";
        showNotification("Có đơn hàng mới " + orderText + " từ " + data.buyerName + " - " + formatCurrency(data.totalAmount), "success");
        // Reload page nếu đang ở trang quản lý đơn hàng
        const path = window.location.pathname.toLowerCase();
        if (path.includes("/seller/orders") || path.includes("/orders")) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
    });

    // Xử lý Listing Status Changed
    connection.on("ListingStatusChanged", (data) => {
        showNotification("Tin đăng #" + data.listingId + ": " + data.message, "info");
        reloadIfOnListingPage();
    });

    // Xử lý Listing Created
    connection.on("ListingCreated", (data) => {
        console.log("ListingCreated received:", data);
        showNotification(data.message, "success");
        reloadIfOnListingPage();
    });

    // Xử lý Listing Updated
    connection.on("ListingUpdated", (data) => {
        console.log("ListingUpdated received:", data);
        showNotification(data.message, "info");
    });

    // Xử lý Listing Public Updated (broadcast cho buyer đang xem)
    connection.on("ListingPublicUpdated", (data) => {
        console.log("ListingPublicUpdated received:", data);
        const listingType = data && data.listingType ? data.listingType : null;
        reloadIfOnListingPage(listingType);
    });

    // Xử lý Listing Hidden
    connection.on("ListingHidden", (data) => {
        console.log("ListingHidden received:", data);
        showNotification(data.message, "info");
        reloadIfOnListingPage();
    });

    // Xử lý Listing Submitted
    connection.on("ListingSubmitted", (data) => {
        showNotification(data.message, "info");
        reloadIfOnListingPage();
    });

    // Xử lý Listing Approved
    connection.on("ListingApproved", (data) => {
        console.log("ListingApproved received:", data);
        showNotification(data.message, "success");
        // Reload ngay lập tức để cập nhật status - reload cho tất cả các trang listings
        reloadIfOnListingPage();
    });

    // Xử lý Listing Rejected
    connection.on("ListingRejected", (data) => {
        console.log("ListingRejected received:", data);
        showNotification(data.message, "error");
        // Reload ngay lập tức để cập nhật status - reload cho tất cả các trang listings
        reloadIfOnListingPage();
    });

    // Xử lý Listing Needs Revision
    connection.on("ListingNeedsRevision", (data) => {
        console.log("ListingNeedsRevision received:", data);
        showNotification(data.message, "warning");
        // Reload ngay lập tức để cập nhật status - reload cho tất cả các trang listings
        reloadIfOnListingPage();
    });

    // Xử lý Listing Deleted
    connection.on("ListingDeleted", (data) => {
        console.log("ListingDeleted received:", data);
        showNotification(data.message, "info");
        // Reload ngay lập tức để xóa listing khỏi danh sách
        if (window.location.pathname.includes("/VehicleListings/Index") || 
            window.location.pathname.includes("/BatteryListings/Index") ||
            window.location.pathname.includes("/VehicleListings/Manage") ||
            window.location.pathname.includes("/BatteryListings/Manage")) {
            setTimeout(() => {
                window.location.reload();
            }, 1500);
        }
    });

    // Xử lý New Pending Listing (cho admin)
    connection.on("NewPendingListing", (data) => {
        showNotification(data.message, "info");
        if (window.location.pathname.includes("/VehicleListings/Pending") || 
            window.location.pathname.includes("/BatteryListings/Pending") ||
            window.location.pathname.includes("/VehicleListings/Manage") ||
            window.location.pathname.includes("/BatteryListings/Manage")) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
    });

    // Helper function để reload nếu đang ở trang quản lý tin đăng
    function reloadIfOnListingPage(listingType) {
        const path = window.location.pathname.toLowerCase();
        const normalizedType = (listingType || "").toLowerCase();
        const pathMatchesListingPage = path.includes("/vehiclelistings") || 
            path.includes("/batterylistings") ||
            path.includes("/vehicle") ||
            path.includes("/battery");
        const manualOptIn = typeof window.enableListingRealtimeReload !== "undefined"
            ? Boolean(window.enableListingRealtimeReload)
            : false;

        if (!pathMatchesListingPage && !manualOptIn) {
            return;
        }

        let typeFilters = [];
        if (Array.isArray(window.listingRealtimeTypes)) {
            typeFilters = window.listingRealtimeTypes
                .map(t => (t || "").toString().toLowerCase())
                .filter(Boolean);
        }

        if (typeFilters.length > 0 && normalizedType && !typeFilters.includes(normalizedType)) {
            return;
        }

        console.log("SignalR: Reloading listing page:", path, "type:", listingType);
        setTimeout(() => {
            window.location.reload();
        }, 1500);
    }

    // Xử lý New Review
    connection.on("NewReview", (data) => {
        console.log("NewReview received:", data);
        showNotification("Bạn có đánh giá mới từ " + data.reviewerName + " (" + data.rating + " sao)", "info");
        // Reload page nếu đang ở trang reviews
        const path = window.location.pathname.toLowerCase();
        if (path.includes("/reviews") || path.includes("/userreviews")) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
    });

    // Xử lý Admin Notification
    connection.on("AdminNotification", (data) => {
        console.log("AdminNotification received:", data);
        showNotification(data.message, data.type || "info");
        // Reload page nếu đang ở trang admin
        const path = window.location.pathname.toLowerCase();
        if (path.includes("/admin")) {
            setTimeout(() => {
                window.location.reload();
            }, 2000);
        }
    });

    // Bắt đầu kết nối
    connection.start()
        .then(() => {
            console.log("SignalR: Đã kết nối");
            isConnected = true;
            joinUserGroups();
        })
        .catch((err) => {
            console.error("SignalR: Lỗi kết nối", err);
        });
}

// Tham gia vào các groups dựa trên user role
function joinUserGroups() {
    if (!isConnected || !connection) {
        console.log("SignalR: Chưa kết nối, không thể join groups");
        return;
    }

    const userId = getUserId();
    const role = getRole();

    console.log("SignalR: Joining groups for userId:", userId, "role:", role);

    if (userId) {
        connection.invoke("JoinUserGroup", userId.toString())
            .then(() => console.log("SignalR: Đã join user group:", userId))
            .catch(err => console.error("Lỗi join user group:", err));
    }

    if (role === "Customer") {
        // Customer có thể là seller hoặc buyer
        if (userId) {
            connection.invoke("JoinSellerGroup", userId.toString())
                .then(() => console.log("SignalR: Đã join seller group:", userId))
                .catch(err => console.error("Lỗi join seller group:", err));
            connection.invoke("JoinBuyerGroup", userId.toString())
                .then(() => console.log("SignalR: Đã join buyer group:", userId))
                .catch(err => console.error("Lỗi join buyer group:", err));
        }
    }

    if (role === "Staff" || role === "Admin") {
        connection.invoke("JoinAdminGroup")
            .then(() => console.log("SignalR: Đã join admin group"))
            .catch(err => console.error("Lỗi join admin group:", err));
    }
}

// Lấy UserId từ session (cần được set từ server)
function getUserId() {
    // Có thể lấy từ data attribute hoặc global variable
    const userIdElement = document.querySelector('[data-user-id]');
    const userId = userIdElement ? userIdElement.getAttribute('data-user-id') : null;
    console.log("SignalR: getUserId() =", userId);
    return userId;
}

// Lấy Role từ session
function getRole() {
    const roleElement = document.querySelector('[data-user-role]');
    const role = roleElement ? roleElement.getAttribute('data-user-role') : null;
    console.log("SignalR: getRole() =", role);
    return role;
}

// Hiển thị notification
function showNotification(message, type = "info") {
    // Tạo toast notification
    const toastContainer = getOrCreateToastContainer();
    
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${getBootstrapColor(type)} border-0`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                <i class="bi bi-${getIcon(type)}"></i> ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    const bsToast = new bootstrap.Toast(toast, {
        autohide: true,
        delay: 5000
    });
    
    bsToast.show();
    
    // Xóa toast sau khi ẩn
    toast.addEventListener('hidden.bs.toast', () => {
        toast.remove();
    });
}

// Lấy hoặc tạo toast container
function getOrCreateToastContainer() {
    let container = document.getElementById('toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toast-container';
        container.className = 'toast-container position-fixed top-0 end-0 p-3';
        container.style.zIndex = '9999';
        document.body.appendChild(container);
    }
    return container;
}

// Chuyển đổi type sang Bootstrap color
function getBootstrapColor(type) {
    const colors = {
        'success': 'success',
        'error': 'danger',
        'warning': 'warning',
        'info': 'info',
        'danger': 'danger'
    };
    return colors[type] || 'info';
}

// Lấy icon theo type
function getIcon(type) {
    const icons = {
        'success': 'check-circle',
        'error': 'exclamation-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle',
        'danger': 'x-circle'
    };
    return icons[type] || 'info-circle';
}

// Format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(amount);
}

// Khởi tạo khi DOM ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', initSignalR);
} else {
    initSignalR();
}

