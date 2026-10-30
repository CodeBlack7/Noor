export type Role = 'Admin' | 'Customer';
export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Processing'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'
  | 'Refunded';
export type PaymentStatus = 'Pending' | 'Paid' | 'Failed' | 'Refunded';
export type PaymentMethod = 'CashOnDelivery';
export type ShippingMethod = 'Standard' | 'Express';
export type CouponType = 'Percentage' | 'FixedAmount';

export interface User {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  role: Role;
  marketingOptIn: boolean;
  createdAt: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: User;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  phone?: string | null;
  marketingOptIn: boolean;
}

export interface UpdateProfileRequest {
  fullName: string;
  phone?: string | null;
  marketingOptIn: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

export interface Category {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  productCount: number;
}

export interface ColorSwatch {
  name: string;
  hex: string;
}

export interface ProductSummary {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  price: number;
  compareAtPrice: number | null;
  categoryId: number;
  categoryName: string;
  categorySlug: string;
  primaryImageUrl: string | null;
  isFeatured: boolean;
  isNew: boolean;
  inStock: boolean;
  colors: ColorSwatch[];
  sizes: string[];
  averageRating: number;
  reviewCount: number;
}

export interface ProductImage {
  id: number;
  url: string;
  altText: string | null;
}

export interface ProductVariant {
  id: number;
  size: string;
  colorName: string;
  colorHex: string;
  sku: string;
  stockQuantity: number;
  price: number;
  priceAdjustment: number;
  inStock: boolean;
}

export interface Review {
  id: number;
  rating: number;
  title: string | null;
  comment: string;
  authorName: string;
  createdAt: string;
}

export interface ReviewList {
  averageRating: number;
  reviewCount: number;
  items: Review[];
}

export interface CreateReviewRequest {
  rating: number;
  title?: string | null;
  comment: string;
}

export interface ProductDetail {
  id: number;
  name: string;
  slug: string;
  shortDescription: string;
  description: string;
  price: number;
  compareAtPrice: number | null;
  categoryId: number;
  categoryName: string;
  categorySlug: string;
  fabric: string;
  careInstructions: string | null;
  isActive: boolean;
  isFeatured: boolean;
  inStock: boolean;
  images: ProductImage[];
  variants: ProductVariant[];
  colors: ColorSwatch[];
  sizes: string[];
  averageRating: number;
  reviewCount: number;
  reviews: Review[];
  relatedProducts: ProductSummary[];
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

export interface HomeContent {
  categories: Category[];
  featured: ProductSummary[];
  newArrivals: ProductSummary[];
  bestsellers: ProductSummary[];
}

export interface ProductFacets {
  minPrice: number;
  maxPrice: number;
  sizes: string[];
  colors: ColorSwatch[];
}

export interface ProductQuery {
  search?: string;
  category?: string;
  minPrice?: number;
  maxPrice?: number;
  size?: string;
  color?: string;
  featured?: boolean;
  sort?: string;
  page?: number;
  pageSize?: number;
}

export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  productSlug: string;
  imageUrl: string | null;
  variantId: number;
  size: string;
  colorName: string;
  colorHex: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  stockQuantity: number;
  lineTotal: number;
}

export interface Cart {
  id: string;
  items: CartItem[];
  subtotal: number;
  discountTotal: number;
  shippingEstimate: number;
  total: number;
  freeShippingRemaining: number;
  couponCode: string | null;
  itemCount: number;
  updatedAt: string;
}

export interface Address {
  id: number;
  label: string;
  fullName: string;
  phone: string;
  line1: string;
  line2: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
}

export interface SaveAddressRequest {
  label: string;
  fullName: string;
  phone: string;
  line1: string;
  line2?: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
}

export interface WishlistItem {
  product: ProductSummary;
  addedAt: string;
}

export interface WishlistToggleResponse {
  inWishlist: boolean;
  message: string;
}

export interface OrderItem {
  id: number;
  productId: number | null;
  productName: string;
  productSlug: string;
  imageUrl: string | null;
  sku: string;
  size: string;
  colorName: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface OrderAddress {
  fullName: string;
  phone: string;
  line1: string;
  line2: string | null;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

export interface OrderSummary {
  id: string;
  orderNumber: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  paymentMethod: PaymentMethod;
  total: number;
  itemCount: number;
  createdAt: string;
  firstItemImage: string | null;
  firstItemName: string | null;
}

export interface OrderDetail {
  id: string;
  orderNumber: string;
  email: string;
  customerName: string;
  status: OrderStatus;
  paymentStatus: PaymentStatus;
  paymentMethod: PaymentMethod;
  shippingMethod: ShippingMethod;
  paymentReference: string | null;
  subtotal: number;
  discountTotal: number;
  shippingTotal: number;
  total: number;
  couponCode: string | null;
  shippingAddress: OrderAddress;
  notes: string | null;
  carrier: string | null;
  trackingNumber: string | null;
  createdAt: string;
  updatedAt: string;
  items: OrderItem[];
}

export interface CheckoutRequest {
  addressId: number;
  shippingMethod: ShippingMethod;
  paymentMethod: PaymentMethod;
  notes?: string | null;
}

export interface NewsletterResponse {
  success: boolean;
  message: string;
}

export interface AdminProductListItem {
  id: number;
  name: string;
  slug: string;
  categoryName: string;
  price: number;
  compareAtPrice: number | null;
  imageUrl: string | null;
  isActive: boolean;
  isFeatured: boolean;
  totalStock: number;
  variantCount: number;
  soldCount: number;
  updatedAt: string;
}

export interface ImageInput {
  url: string;
  altText?: string | null;
}

export interface VariantInput {
  id?: number | null;
  size: string;
  colorName: string;
  colorHex: string;
  sku: string;
  stockQuantity: number;
  priceAdjustment: number;
}

export interface ProductUpsertRequest {
  name: string;
  slug?: string | null;
  categoryId: number;
  shortDescription: string;
  description: string;
  price: number;
  compareAtPrice: number | null;
  fabric: string;
  careInstructions?: string | null;
  isActive: boolean;
  isFeatured: boolean;
  images: ImageInput[];
  variants: VariantInput[];
}

export interface AdminCategory {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  isActive: boolean;
  sortOrder: number;
  productCount: number;
}

export interface CategoryUpsertRequest {
  name: string;
  slug?: string | null;
  description?: string | null;
  imageUrl?: string | null;
  isActive: boolean;
  sortOrder: number;
}

export interface Coupon {
  id: number;
  code: string;
  description: string | null;
  discountType: CouponType;
  value: number;
  minOrderAmount: number;
  maxRedemptions: number | null;
  timesRedeemed: number;
  startsAt: string | null;
  endsAt: string | null;
  isActive: boolean;
  isLive: boolean;
}

export interface CouponUpsertRequest {
  code: string;
  description?: string | null;
  discountType: CouponType;
  value: number;
  minOrderAmount: number;
  maxRedemptions?: number | null;
  startsAt?: string | null;
  endsAt?: string | null;
  isActive: boolean;
}

export interface AdminCustomer {
  id: string;
  fullName: string;
  email: string;
  phone: string | null;
  isActive: boolean;
  marketingOptIn: boolean;
  orderCount: number;
  totalSpent: number;
  createdAt: string;
  lastLoginAt: string | null;
}

export interface SalesPoint {
  date: string;
  revenue: number;
  orders: number;
}

export interface TopProduct {
  productId: number;
  name: string;
  imageUrl: string | null;
  unitsSold: number;
  revenue: number;
}

export interface LowStockItem {
  variantId: number;
  productName: string;
  sku: string;
  size: string;
  colorName: string;
  stockQuantity: number;
}

export interface Dashboard {
  totalRevenue: number;
  revenueThisMonth: number;
  totalOrders: number;
  ordersThisMonth: number;
  pendingOrders: number;
  totalCustomers: number;
  totalProducts: number;
  lowStockCount: number;
  newsletterSubscribers: number;
  recentOrders: OrderSummary[];
  topProducts: TopProduct[];
  salesByDay: SalesPoint[];
  lowStockVariants: LowStockItem[];
}

export interface UploadResponse {
  url: string;
  fileName: string;
}

export interface AdminOrderQuery {
  search?: string;
  status?: OrderStatus;
  page?: number;
  pageSize?: number;
}

export interface AdminProductQuery {
  search?: string;
  categoryId?: number;
  isActive?: boolean;
  page?: number;
  pageSize?: number;
}

export interface AdminCustomerQuery {
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
  paymentStatus?: PaymentStatus | null;
  carrier?: string | null;
  trackingNumber?: string | null;
}
