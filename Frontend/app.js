
// =================================
// FOODIEHUB SHOPPING CART
// =================================

const API_URL = "http://localhost:5031/api";

let cart = [];


// 1. ADD FOOD TO CART
function addToCart(productId, name, price) {

    let existingItem = cart.find(
        item => item.productId === productId
    );

    if (existingItem) {

        existingItem.quantity++;

    } else {

        cart.push({
            productId: productId,
            name: name,
            price: price,
            quantity: 1
        });

    }

    updateCart();
}


// 2. UPDATE CART
function updateCart() {

    let cartContainer = document.getElementById("cart-items");

    let totalItems = 0;
    let totalPrice = 0;

    cartContainer.innerHTML = "";

    if (cart.length === 0) {

        cartContainer.innerHTML =
            '<p class="empty-cart">Your cart is empty! Add some delicious food. 🍔</p>';

    }

    cart.forEach((item, index) => {

        let subtotal = item.price * item.quantity;

        totalItems += item.quantity;
        totalPrice += subtotal;

        cartContainer.innerHTML += `

            <div class="cart-item">

                <div>
                    <h3>${item.name}</h3>
                    <p>Rs. ${item.price.toLocaleString()}</p>
                </div>

                <div class="quantity-controls">

                    <button onclick="changeQuantity(${index}, -1)">-</button>

                    <span>${item.quantity}</span>

                    <button onclick="changeQuantity(${index}, 1)">+</button>

                </div>

                <div>
                    <strong>Rs. ${subtotal.toLocaleString()}</strong>
                </div>

                <button class="remove-btn"
                    onclick="removeItem(${index})">
                    Remove
                </button>

            </div>

        `;

    });

    document.getElementById("total-items").textContent = totalItems;

    document.getElementById("total-price").textContent =
        "Rs. " + totalPrice.toLocaleString();

    document.getElementById("cart-count").textContent = totalItems;

}


// 3. CHANGE QUANTITY
function changeQuantity(index, amount) {

    cart[index].quantity += amount;

    if (cart[index].quantity <= 0) {

        cart.splice(index, 1);

    }

    updateCart();

}


// 4. REMOVE ITEM
function removeItem(index) {

    cart.splice(index, 1);

    updateCart();

}


// 5. SHOW CART
function showCart() {

    document.getElementById("cart-section").scrollIntoView({
        behavior: "smooth"
    });

}


// 6. GO TO CHECKOUT
function goToCheckout() {

    if (cart.length === 0) {

        alert("Your cart is empty! Please add some food.");

        return;

    }

    document.getElementById("checkout").scrollIntoView({
        behavior: "smooth"
    });

}


// 7. FILTER FOOD CATEGORIES
function filterFood(category) {

    let foodCards = document.querySelectorAll(".food-card");

    foodCards.forEach(card => {

        let foodCategory = card.dataset.category;

        if (category === "All" || foodCategory === category) {

            card.style.display = "block";

        } else {

            card.style.display = "none";

        }

    });

}


// 8. PLACE ORDER - CONNECT TO DATABASE
document.getElementById("checkout-form").addEventListener(
    "submit",
    async function(event) {

        event.preventDefault();

        // Check whether cart is empty
        if (cart.length === 0) {

            alert("Your cart is empty!");

            return;

        }

        // Get customer details
        const customerName =
            document.getElementById("customer-name").value.trim();

        const customerPhone =
            document.getElementById("customer-phone").value.trim();

        const customerAddress =
            document.getElementById("customer-address").value.trim();

        const paymentMethod =
            document.getElementById("payment-method").value;

        // Prepare order data for backend
        const orderData = {

            fullName: customerName,

            email: null,

            phoneNumber: customerPhone,

            deliveryAddress: customerAddress,

            paymentMethod: paymentMethod,

            items: cart.map(item => ({

                productId: item.productId,

                quantity: item.quantity

            }))

        };

        // Disable button while placing order
        const submitButton = event.submitter;

        if (submitButton) {
            submitButton.disabled = true;
            submitButton.textContent = "Placing Order...";
        }

        try {

            // Send order to ASP.NET Core API
            const response = await fetch(`${API_URL}/orders`, {

                method: "POST",

                headers: {
                    "Content-Type": "application/json"
                },

                body: JSON.stringify(orderData)

            });

            const result = await response.json();

            // Check API response
            if (!response.ok) {

                throw new Error(
                    typeof result === "string"
                        ? result
                        : result.message || "Unable to place order."
                );

            }

            // Show successful order confirmation
            alert(

                "ORDER PLACED SUCCESSFULLY! 🎉\n\n" +

                "Order ID: " + result.orderId + "\n" +

                "Customer: " + result.customerName + "\n" +

                "Total: Rs. " +
                Number(result.totalAmount).toLocaleString() + "\n" +

                "Status: " + result.status + "\n\n" +

                "Thank you for ordering from FoodieHub!"

            );

            // Clear cart after successful order
            cart = [];

            updateCart();

            // Reset checkout form
            document.getElementById("checkout-form").reset();

        } catch (error) {

            console.error("Order Error:", error);

            alert(
                "ORDER FAILED! ❌\n\n" +
                error.message +
                "\n\nPlease try again."
            );

        } finally {

            // Enable button again
            if (submitButton) {
                submitButton.disabled = false;
                submitButton.textContent = "Place Order";
            }

        }

    }
);


// =================================
// FOODIEHUB API CONNECTION
// =================================


// 9. LOAD CATEGORIES
async function loadCategories() {

    try {

        const response = await fetch(`${API_URL}/categories`);

        if (!response.ok) {
            throw new Error("Failed to load categories");
        }

        const categories = await response.json();

        const container = document.getElementById("category-container");

        categories.forEach(category => {

            const button = document.createElement("button");

            button.className = "category-card";

            button.innerHTML = `<span>${category.name}</span>`;

            button.onclick = () => filterFood(category.name);

            container.appendChild(button);

        });

    } catch (error) {

        console.error("Category Error:", error);

    }

}


// 10. LOAD PRODUCTS
async function loadProducts() {

    try {

        const response = await fetch(`${API_URL}/products`);

        if (!response.ok) {
            throw new Error("Failed to load products");
        }

        const products = await response.json();

        const container = document.getElementById("food-container");

        container.innerHTML = "";

        products.forEach(product => {

            if (!product.isAvailable) {
                return;
            }

            const card = document.createElement("div");

            card.className = "food-card";

            card.dataset.category = product.category?.name || "";

            card.innerHTML = `

                <img src="${product.imageUrl}"
                     alt="${product.name}">

                <div class="food-info">

                    <h3>${product.name}</h3>

                    <p>${product.description}</p>

                    <div class="food-bottom">

                        <strong>
                            Rs. ${Number(product.price).toLocaleString()}
                        </strong>

                        <button>Add</button>

                    </div>

                </div>

            `;

            // Pass Product ID, Name and Price
            card.querySelector("button").onclick = () => {

                addToCart(
                    product.id,
                    product.name,
                    Number(product.price)
                );

            };

            container.appendChild(card);

        });

    } catch (error) {

        console.error("Product Error:", error);

        document.getElementById("food-container").innerHTML =
            "<p>Unable to load food items. Please try again.</p>";

    }

}


// 11. LOAD DATA WHEN WEBSITE OPENS
updateCart();

loadCategories();

loadProducts();
