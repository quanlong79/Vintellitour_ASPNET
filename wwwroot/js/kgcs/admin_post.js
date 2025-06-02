
const btnValidateAll = document.getElementById('btnValidateAll');
const tabs = document.querySelectorAll('.tab-btn');
const posts = document.querySelectorAll('.post-card');

let tempFlaggedPosts = new Set();
let isConfirmMode = false;

function updateCounts() {
    const normalCount = document.querySelectorAll('.post-card[data-status]:not([data-status="flagged"]):not([data-temp-flagged])').length;
    const tempFlaggedCount = document.querySelectorAll('.post-card[data-temp-flagged]').length;
    const flaggedCount = document.querySelectorAll('.post-card[data-status="flagged"]:not([data-temp-flagged])').length;
    const totalCount = posts.length;

    document.getElementById('normal-count').textContent = normalCount;
    document.getElementById('flagged-count').textContent = tempFlaggedCount;
    document.getElementById('history-count').textContent = flaggedCount;

    document.getElementById('stats-normal').textContent = normalCount;
    document.getElementById('stats-temp-flagged').textContent = tempFlaggedCount;
    document.getElementById('stats-flagged').textContent = flaggedCount;
    document.getElementById('stats-total').textContent = totalCount;
}

function showPostsForTab(tab) {
    posts.forEach(post => {
        const status = post.getAttribute('data-status');
        const isTempFlagged = post.hasAttribute('data-temp-flagged');

        if (tab === 'normal') {
            post.style.display = (status !== 'flagged' && !isTempFlagged) ? 'block' : 'none';
        } else if (tab === 'flagged') {
            post.style.display = isTempFlagged ? 'block' : 'none';
        } else if (tab === 'history') {
            post.style.display = (status === 'flagged' && !isTempFlagged) ? 'block' : 'none';
        }
    });

    tabs.forEach(t => {
        const tabType = t.getAttribute('data-tab');
        t.className = 'tab-btn px-6 py-3 rounded-md font-medium text-sm flex items-center';

        if (tabType === tab) {
            if (tab === 'normal') {
                t.classList.add('text-white', 'bg-blue-600');
            } else if (tab === 'flagged') {
                t.classList.add('text-white', 'bg-orange-600');
            } else {
                t.classList.add('text-white', 'bg-red-600');
            }
        } else {
            t.classList.add('text-gray-700', 'bg-transparent', 'hover:bg-gray-50');
        }
    });

    updateCounts();
}

showPostsForTab('normal');

tabs.forEach(tabBtn => {
    tabBtn.addEventListener('click', () => {
        const tab = tabBtn.getAttribute('data-tab');
        showPostsForTab(tab);
    });
});

btnValidateAll.addEventListener('click', async () => {
    if (!isConfirmMode) {
        // Chế độ kiểm duyệt
        btnValidateAll.disabled = true;
        btnValidateAll.innerHTML = `
    <svg class="animate-spin -ml-1 mr-3 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
    </svg>
    Đang kiểm duyệt...
    `;

        try {
            const response = await fetch('/api/contentvalidation/validate');
            const result = await response.json();

            if (result.success) {
                // Xóa flagged tạm cũ
                tempFlaggedPosts.forEach(postId => {
                    const oldPostEl = document.querySelector(`[data-post-id="${postId}"]`);
                    if (oldPostEl) {
                        oldPostEl.removeAttribute('data-temp-flagged');
                        const badge = oldPostEl.querySelector('.flagged-badge.temp-flagged-badge');
                        if (badge) badge.remove();
                    }
                });
                tempFlaggedPosts.clear();

                if (result.invalidPosts && result.invalidPosts.length > 0) {
                    result.invalidPosts.forEach(postInfo => {
                        const postEl = document.querySelector(`[data-post-id="${postInfo.postId}"]`);
                        if (postEl) {
                            postEl.setAttribute('data-temp-flagged', 'true');

                            let badge = postEl.querySelector('.flagged-badge');
                            if (!badge) {
                                badge = document.createElement('div');
                                badge.className = 'flagged-badge temp-flagged-badge';
                                postEl.querySelector('.relative').appendChild(badge);
                            } else {
                                badge.classList.add('temp-flagged-badge');
                            }
                            badge.textContent = postInfo.label || 'Vi phạm';

                            tempFlaggedPosts.add(postInfo.postId);
                        }
                    });


                    document.getElementById('tab-flagged').click();

                    // Chuyển nút sang xác nhận
                    isConfirmMode = true;
                    btnValidateAll.innerHTML = `
    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
    </svg>
    Xác nhận tất cả (${tempFlaggedPosts.size})
    `;
                    btnValidateAll.className = 'ml-auto px-6 py-3 rounded-lg text-sm font-medium bg-gradient-to-r from-green-500 to-green-600 text-white hover:from-green-600 hover:to-green-700 shadow-lg hover:shadow-xl transition-all duration-200 flex items-center';

                    showToast(`Đã phát hiện ${result.invalidCount} bài viết vi phạm cần kiểm duyệt.`, 'info');
                } else {
                    showToast('Không có bài viết vi phạm mới.', 'info');
                }
            } else {
                showToast('Kiểm duyệt thất bại: ' + (result.message || 'Lỗi không xác định'), 'error');
            }
        } catch (error) {
            showToast('Lỗi khi kiểm duyệt: ' + error.message, 'error');
        } finally {
            btnValidateAll.disabled = false;
            if (!isConfirmMode) {
                btnValidateAll.innerHTML = `
              <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
              </svg>
              Kiểm duyệt tất cả bài viết
            `;
            }
        }
    } else {
        // Chế độ xác nhận
        if (tempFlaggedPosts.size === 0) {
            showToast('Không có bài viết cần xác nhận.', 'info');
            return;
        }

        const confirmed = await showConfirm(`Bạn có chắc chắn muốn xác nhận tất cả ${tempFlaggedPosts.size} bài viết vi phạm không?`);
        if (!confirmed) return;

        btnValidateAll.disabled = true;
        btnValidateAll.innerHTML = `
    <svg class="animate-spin -ml-1 mr-3 h-4 w-4 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
        <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle>
        <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
    </svg>
    Đang xác nhận...
    `;

        try {
            const response = await fetch('/post/confirm-flagged', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(Array.from(tempFlaggedPosts))
            });

            const result = await response.json();
            if (result.success) {
                showToast(`Đã xác nhận ${tempFlaggedPosts.size} bài viết vi phạm.`, 'success');
                tempFlaggedPosts.forEach(postId => {
                    const postEl = document.querySelector(`[data-post-id="${postId}"]`);
                    if (postEl) {
                        postEl.removeAttribute('data-temp-flagged');
                        postEl.setAttribute('data-status', 'flagged');

                        const badge = postEl.querySelector('.flagged-badge');
                        if (badge) {
                            badge.classList.remove('temp-flagged-badge');
                        }
                    }
                });

                tempFlaggedPosts.clear();

                document.getElementById('tab-history').click();

                isConfirmMode = false;
                btnValidateAll.innerHTML = `
    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 mr-2" fill="none" viewBox="0 0 24 24" stroke="currentColor">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v10a2 2 0 002 2h8a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4" />
    </svg>
    Kiểm duyệt tất cả bài viết
    `;
                btnValidateAll.className = 'ml-auto px-6 py-3 rounded-lg text-sm font-medium bg-gradient-to-r from-orange-500 to-orange-600 text-white hover:from-orange-600 hover:to-orange-700 shadow-lg hover:shadow-xl transition-all duration-200 flex items-center';
            } else {
                showToast('error', 'Xác nhận thất bại: ' + (result.message || 'Lỗi không xác định'));
            }
        } catch (error) {
            showToast('error', 'Lỗi khi xác nhận: ' + error.message);
        } finally {
            btnValidateAll.disabled = false;
        }
    }
});
document.querySelectorAll('.btn-read-more').forEach(button => {
    button.addEventListener('click', () => {
        const container = button.closest('.post-content');
        const shortText = container.querySelector('.short-text');
        const fullText = container.querySelector('.full-text');

        if (fullText.classList.contains('hidden')) {
            fullText.classList.remove('hidden');
            shortText.classList.add('hidden');
            button.textContent = 'Thu gọn';
        } else {
            fullText.classList.add('hidden');
            shortText.classList.remove('hidden');
            button.textContent = 'Xem thêm';
        }
    });
});
document.querySelectorAll('.delete-btn').forEach(btn => {
    btn.addEventListener('click', async (e) => {
        e.preventDefault(); // Ngăn submit form hoặc reload trang
        const postId = btn.getAttribute('data-post-id');
        if (!postId) return;

        const confirmed = await showConfirm('Bạn có chắc chắn muốn xóa bài viết này?');
        if (!confirmed) return;

        btn.disabled = true;
        const originalHtml = btn.innerHTML;
        btn.innerHTML = '<svg class="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>';

        try {
            const response = await fetch(`/post/posts/${postId}`, { method: 'DELETE' });
            if (response.ok) {
                const postCard = btn.closest('.post-card');
                if (postCard) {
                    postCard.style.transition = 'all 0.3s ease';
                    postCard.style.opacity = '0';
                    postCard.style.transform = 'scale(0.95)';
                    setTimeout(() => {
                        postCard.remove();
                        updateCounts();
                    }, 300);
                }
                showToast('Xóa bài viết thành công.', 'success');
            } else {
                alert('Xóa bài viết thất bại. Vui lòng thử lại.');
                btn.innerHTML = originalHtml;
                btn.disabled = false;
            }
        } catch (error) {
            alert('Lỗi khi xóa bài viết: ' + error.message);
            btn.innerHTML = originalHtml;
            btn.disabled = false;
        }
    });
});
document.querySelectorAll('.approve-form').forEach(form => {
    form.addEventListener('submit', async e => {
        e.preventDefault();

        const submitBtn = form.querySelector('button[type="submit"]');
        submitBtn.disabled = true;
        const originalHtml = submitBtn.innerHTML;
        submitBtn.innerHTML = '<svg class="animate-spin w-4 h-4" fill="none" viewBox="0 0 24 24"><circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"></circle><path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path></svg>';

        try {
            const response = await fetch(form.action, {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
                },
                body: new URLSearchParams(new FormData(form))
            });

            const result = await response.json();
            if (result.success) {
                showToast('Đã khôi phục trạng thái bài viết thành "Đã duyệt".', 'success');
                const postCard = form.closest('.post-card');
                if (postCard) {
                    postCard.setAttribute('data-status', 'approved');
                    postCard.style.transition = 'all 0.3s ease';
                    postCard.style.opacity = '0';
                    postCard.style.transform = 'scale(0.95)';

                    setTimeout(() => {
                        postCard.remove();
                        updateCounts();
                    }, 300);
                }
            } else {
                showToast('error', 'Duyệt bài viết thất bại: ' + (result.message || 'Lỗi không xác định'));
                submitBtn.innerHTML = originalHtml;
                submitBtn.disabled = false;
            }
        } catch (err) {
            showToast('error', 'Lỗi khi duyệt bài viết: ' + err.message);
            submitBtn.innerHTML = originalHtml;
            submitBtn.disabled = false;
        }
    });
});
function showToast(message, type = 'info', duration = 3000) {
    const container = document.getElementById('toast-container');
    if (!container) return;

    const toast = document.createElement('div');
    toast.className = `toast toast-${type}`;
    toast.textContent = message;

    container.appendChild(toast);

    setTimeout(() => {
        toast.classList.add('fade-out');
        toast.addEventListener('transitionend', () => toast.remove());
    }, duration);
}
function showConfirm(message) {
    return new Promise((resolve) => {
        const modal = document.getElementById('custom-confirm');
        const msgEl = document.getElementById('custom-confirm-message');
        const btnOk = document.getElementById('custom-confirm-ok');
        const btnCancel = document.getElementById('custom-confirm-cancel');

        msgEl.textContent = message;

        modal.classList.remove('hidden');

        function cleanup() {
            btnOk.removeEventListener('click', onOk);
            btnCancel.removeEventListener('click', onCancel);
            modal.classList.add('hidden');
        }

        function onOk() {
            cleanup();
            resolve(true);
        }

        function onCancel() {
            cleanup();
            resolve(false);
        }

        btnOk.addEventListener('click', onOk);
        btnCancel.addEventListener('click', onCancel);
    });
}
