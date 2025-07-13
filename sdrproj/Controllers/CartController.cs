using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdrproj.Models;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<IActionResult> ViewProduct(string searchString)
    {
        IQueryable<Product> products = _context.Products.Include(p => p.SubCategory);

        if (!string.IsNullOrEmpty(searchString))
        {
            products = products.Where(p => p.Name.Contains(searchString) ||
                                           p.Description.Contains(searchString));
        }

        return View(await products.ToListAsync());
    }

    public async Task<IActionResult> Detail(int? id)
    {
        if (id == null) return NotFound();

        var product = await _context.Products
            .Include(p => p.SubCategory)
            .FirstOrDefaultAsync(p => p.ProductId == id);

        if (product == null) return NotFound();

        return View(product);
    }


    private int GetOrCreateSessionUserId()
    {
        // Simulate logged-in user. Replace with real authentication in production.
        return 1;
    }

    // Wishlist methods
    public async Task<IActionResult> Add(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            TempData["WishlistMessage"] = "Product not found.";
            return RedirectToAction("ViewProduct");
        }

        bool exists = _context.WishList.Any(w => w.ProductId == id);
        if (!exists)
        {
            var wishlistItem = new WishList
            {
                ProductId = id,
                TimeAdd = DateTime.Now,
                UserId = GetOrCreateSessionUserId()
            };

            _context.WishList.Add(wishlistItem);
            await _context.SaveChangesAsync();

            TempData["WishlistMessage"] = "Product added to wishlist!";
        }
        else
        {
            TempData["WishlistMessage"] = "Product already in wishlist.";
        }

        return RedirectToAction("Detail", "Home", new { id = id });
    }

    public async Task<IActionResult> ViewWishlist()
    {
        var wishlistItems = await _context.WishList
            .Include(w => w.Product)
            .ToListAsync();

        return View(wishlistItems);
    }

    public async Task<IActionResult> Remove(int id)
    {
        var wishlistItem = await _context.WishList.FindAsync(id);
        if (wishlistItem == null)
        {
            TempData["WishlistMessage"] = "Item not found in wishlist.";
            return RedirectToAction("ViewWishlist");
        }

        _context.WishList.Remove(wishlistItem);
        await _context.SaveChangesAsync();

        TempData["WishlistMessage"] = "Item removed from wishlist.";
        return RedirectToAction("ViewWishlist");
    }

    private int GetUserId()
    {
        var userIdString = HttpContext.Session.GetString("UserId");
        if (string.IsNullOrEmpty(userIdString))
        {
            throw new UnauthorizedAccessException("User not logged in.");
        }

        return int.Parse(userIdString);
    }


    public async Task<IActionResult> ViewCart()
    {
        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in to view your cart.";
            return RedirectToAction("Login", "UserAccount");
        }
        var cartItems = await _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        ViewBag.CartTotal = cartItems.Sum(c => c.Product.Price * c.Count);
        ViewBag.ItemCount = cartItems.Sum(c => c.Count);

        return View(cartItems);
    }

    public async Task<IActionResult> AddToCart(int productId, int count = 1)
    {

        var product = await _context.Products.FindAsync(productId);
        if (product == null || product.Stock < count)
        {
            TempData["CartMessage"] = "Product not found or insufficient stock.";
            return RedirectToAction("ViewProduct", "Home");
        }

        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in to view your cart.";
            return RedirectToAction("Login", "UserAccount");
        }
        var existingCart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

        if (existingCart != null)
        {
            if (existingCart.Count + count > product.Stock)
            {
                TempData["CartMessage"] = "Not enough stock.";
                return RedirectToAction("ViewProduct", "Home");
            }

            existingCart.Count += count;
            existingCart.AddedDateTime = DateTime.Now;
            _context.Carts.Update(existingCart);
        }
        else
        {
            _context.Carts.Add(new Cart
            {
                UserId = userId,
                ProductId = productId,
                Count = count,
                AddedDateTime = DateTime.Now
            });
        }

        product.Stock -= count;
        await _context.SaveChangesAsync();

        TempData["CartMessage"] = "Product added to cart!";
        return RedirectToAction("ViewCart");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCartQuantity(int cartId, int count)
    {
        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in to view your cart.";
            return RedirectToAction("Login", "UserAccount");
        }
        if (count < 1)
            return await RemoveFromCart(cartId);

        var cartItem = await _context.Carts.Include(c => c.Product).FirstOrDefaultAsync(c => c.CartId == cartId);
        if (cartItem == null)
        {
            TempData["CartMessage"] = "Cart item not found.";
            return RedirectToAction("ViewCart");
        }

        if (count > cartItem.Product.Stock)
        {
            TempData["CartMessage"] = "Not enough stock.";
            return RedirectToAction("ViewCart");
        }

        cartItem.Count = count;
        cartItem.AddedDateTime = DateTime.Now;
        _context.Carts.Update(cartItem);
        await _context.SaveChangesAsync();

        TempData["CartMessage"] = "Cart updated.";
        return RedirectToAction("ViewCart");
    }

    public async Task<IActionResult> RemoveFromCart(int id)
    {
        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in to view your cart.";
            return RedirectToAction("Login", "UserAccount");
        }
        var cartItem = await _context.Carts.FindAsync(id);
        if (cartItem == null)
        {
            TempData["CartMessage"] = "Item not found.";
            return RedirectToAction("ViewCart");
        }

        _context.Carts.Remove(cartItem);
        await _context.SaveChangesAsync();

        TempData["CartMessage"] = "Item removed.";
        return RedirectToAction("ViewCart");
    }

    public async Task<IActionResult> ClearCart()
    {
        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in to view your cart.";
            return RedirectToAction("Login", "UserAccount");
        }
  
        var cartItems = await _context.Carts.Where(c => c.UserId == userId).ToListAsync();

        _context.Carts.RemoveRange(cartItems);
        await _context.SaveChangesAsync();

        TempData["CartMessage"] = "Cart cleared!";
        return RedirectToAction("ViewCart");
    }

    public async Task<IActionResult> GetCartItemCount()
    {
        int userId = GetUserId();
        int count = await _context.Carts.Where(c => c.UserId == userId).SumAsync(c => c.Count);
        return Json(count);
    }
    public async Task<IActionResult> PlaceOrder()
    {
        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in to view your cart.";
            return RedirectToAction("Login", "UserAccount");
        }

        // Load user's cart with product details
        var cartItems = await _context.Carts
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
        {
            TempData["CartMessage"] = "Your cart is empty.";
            return RedirectToAction("ViewCart");
        }

        decimal totalAmount = cartItems.Sum(item => item.Product.Price * item.Count);

        // Create order header
        var orderHeader = new OrderHeader
        {
            UserId = userId,
            OrderDate = DateTime.Now,
            TotalAmount = totalAmount,
            OrderStatus = "Pending",
            PaymentStatus = "Unpaid"
            // TrackingNumber, PaymentDate, ReceiptUrl will be set later after payment
        };

        _context.OrderHeaders.Add(orderHeader);
        await _context.SaveChangesAsync(); // Save to generate OrderHeaderId

        // Add order details and adjust stock
        foreach (var item in cartItems)
        {
            var orderDetail = new OrderDetail
            {
                OrderHeaderId = orderHeader.OrderHeaderId,
                ProductId = item.ProductId,
                Quantity = item.Count,
                UnitPrice = item.Product.Price
            };

            _context.OrderDetails.Add(orderDetail);

            // Reduce product stock
            item.Product.Stock -= item.Count;
            _context.Products.Update(item.Product);
        }

        // Remove items from cart
        _context.Carts.RemoveRange(cartItems);

        // Save all changes
        await _context.SaveChangesAsync();

        TempData["CartMessage"] = $"✅ Order placed successfully! Order ID: {orderHeader.OrderHeaderId}";
        return RedirectToAction("ViewOrders");
    }
    public async Task<IActionResult> ViewOrders()
    {
        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in to view your cart.";
            return RedirectToAction("Login", "UserAccount");
        }
        var orders = await _context.OrderHeaders
            .Where(o => o.UserId == userId)
            .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Product)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return View(orders);
    }
    public async Task<IActionResult> CancelOrder(int id)
    {
        int userId;
        try
        {
            userId = GetUserId();
        }
        catch
        {
            TempData["Error"] = "You must be logged in.";
            return RedirectToAction("Login", "UserAccount");
        }

        var order = await _context.OrderHeaders
            .Include(o => o.OrderDetails)
            .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(o => o.OrderHeaderId == id && o.UserId == userId);

        if (order == null)
        {
            TempData["CartMessage"] = "Order not found.";
            return RedirectToAction("ViewOrders");
        }

        if (order.OrderStatus == "Cancelled")
        {
            TempData["CartMessage"] = "Order already cancelled.";
            return RedirectToAction("ViewOrders");
        }

        // Restore product stock
        foreach (var detail in order.OrderDetails)
        {
            detail.Product.Stock += detail.Quantity;
            _context.Products.Update(detail.Product);
        }

        // Update order status
        order.OrderStatus = "Cancelled";
        order.PaymentStatus = "Refunded"; // optional
        _context.OrderHeaders.Update(order);

        await _context.SaveChangesAsync();

        TempData["CartMessage"] = $"Order #{id} cancelled.";
        return RedirectToAction("ViewOrders");
    }

}
