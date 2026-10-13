Auth
		 ✅POST /api/auth/register | Anyone | 200 + token
		 ✅POST /api/auth/register (invalid email) | Anyone | 400
		 ✅POST /api/auth/login | Anyone | 200 + token
		 ✅POST /api/auth/login (wrong password) | Anyone | 401
Categories
		 ✅GET /api/categories | Anyone | 200
		 ✅POST /api/categories | Admin | 201
		 ✅PUT /api/categories/{id} (same values) | Admin | 200
Products
			✅ GET /api/products | Anyone | 200
			 ✅GET /api/products?search=key | Anyone | 200
			✅GET /api/products/999999 | Anyone | 404
			✅ POST /api/products | Admin | 201
			✅ POST /api/products (isActive:false, id:99) | Admin | 201, still active
			 ✅POST /api/products (no token) | Anyone | 401
			 POST /api/products (Customer token) | Customer | 403
			✅ POST /api/products (negative price) | Admin | 400
			✅ POST /api/products (categoryId 999) | Admin | 400
			✅ DELETE /api/products/{id} | Admin | 204
			✅ POST /api/products/{id}/stock (+20) | Admin | 200
			 ✅POST /api/products/{id}/stock (negative result) | Admin | 400
			 ✅GET /api/products/low-stock | Admin | 200
Cart
			✅ GET /api/cart | User | 200
			✅GET /api/cart (no token) | Anyone | 401
			 ✅POST /api/cart/items | User | 200
			 ✅POST /api/cart/items (same product again) | User | 200, qty merged
			✅ POST /api/cart/items (qty > stock) | User | 400
			✅ POST /api/cart/items (qty -1) | User | 400
			✅ DELETE /api/cart/items/{id} | User | 204
Orders
			 ✅POST /api/orders/checkout | User | 201
			  ✅POST /api/orders/checkout (empty cart) | User | 400
			 ✅GET /api/orders | User | 200
			 ✅GET /api/orders/{id} (other user's order) | User | 404