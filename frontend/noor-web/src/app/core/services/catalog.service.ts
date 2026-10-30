import { inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { API_BASE } from '../api.config';
import {
  Category,
  CreateReviewRequest,
  HomeContent,
  NewsletterResponse,
  PagedResult,
  ProductDetail,
  ProductFacets,
  ProductQuery,
  ProductSummary,
  Review,
  ReviewList
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private readonly http = inject(HttpClient);
  private readonly categoriesState = signal<Category[]>([]);

  readonly categories = this.categoriesState.asReadonly();

  loadCategories(): void {
    if (this.categoriesState().length > 0) return;
    this.http.get<Category[]>(`${API_BASE}/categories`).subscribe({
      next: (categories) => this.categoriesState.set(categories),
      error: () => undefined
    });
  }

  getHome(): Observable<HomeContent> {
    return this.http
      .get<HomeContent>(`${API_BASE}/home`)
      .pipe(tap((home) => this.categoriesState.set(home.categories)));
  }

  getProducts(query: ProductQuery): Observable<PagedResult<ProductSummary>> {
    return this.http.get<PagedResult<ProductSummary>>(`${API_BASE}/products`, {
      params: this.toParams(query)
    });
  }

  getProduct(slug: string): Observable<ProductDetail> {
    return this.http.get<ProductDetail>(`${API_BASE}/products/${encodeURIComponent(slug)}`);
  }

  getFacets(): Observable<ProductFacets> {
    return this.http.get<ProductFacets>(`${API_BASE}/products/facets`);
  }

  getReviews(productId: number): Observable<ReviewList> {
    return this.http.get<ReviewList>(`${API_BASE}/products/${productId}/reviews`);
  }

  addReview(productId: number, request: CreateReviewRequest): Observable<Review> {
    return this.http.post<Review>(`${API_BASE}/products/${productId}/reviews`, request);
  }

  subscribeNewsletter(email: string): Observable<NewsletterResponse> {
    return this.http.post<NewsletterResponse>(`${API_BASE}/newsletter`, { email });
  }

  private toParams(query: ProductQuery): HttpParams {
    let params = new HttpParams();
    const add = (key: string, value: unknown): void => {
      if (value === undefined || value === null || value === '') return;
      params = params.set(key, String(value));
    };

    add('search', query.search);
    add('category', query.category);
    add('minPrice', query.minPrice);
    add('maxPrice', query.maxPrice);
    add('size', query.size);
    add('color', query.color);
    add('featured', query.featured);
    add('sort', query.sort);
    add('page', query.page);
    add('pageSize', query.pageSize);
    return params;
  }
}
