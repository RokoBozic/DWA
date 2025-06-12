
document.getElementById("profileForm").addEventListener("submit", function (e) {
    e.preventDefault();
    const formData = new FormData(e.target);
    const data = {};
    formData.forEach((value, key) => data[key] = value);

    fetch('/Profile/Update', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data)
    })
    .then(res => res.ok ? res.json() : Promise.reject("Failed to update"))
    .then(res => {
        document.getElementById("resultMessage").innerText = "Profile updated successfully!";
    })
    .catch(err => {
        document.getElementById("resultMessage").innerText = "Error updating profile.";
    });
});
