import { Injectable, computed, inject, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Subject, switchMap, tap } from "rxjs";
import { TagsApiService } from "../services/tags-api.service";
import { PaginatedResult } from "../models/paginated-result.model";
import { RefreshResult } from "../models/refresh-result.model";
import { SortField } from "../models/sort-field.model";
import { Tag } from "../models/tag.model";
import { TagsQuery } from "../models/tags-query.model";

export type LoadState = "idle" | "loading" | "refreshing" | "error";

@Injectable({ providedIn: "root" })
export class TagsStore {
  readonly #api = inject(TagsApiService);

  // Private state
  readonly #result = signal<PaginatedResult<Tag> | null>(null);
  readonly #loadState = signal<LoadState>("idle");
  readonly #error = signal<string | null>(null);
  readonly #refreshMsg = signal<string | null>(null);

  readonly #query = signal<TagsQuery>({
    page: 1,
    pageSize: 25,
    sortBy: "name",
    sortOrder: "asc",
  });

  // Public selectors
  readonly tags = computed(() => this.#result()?.items ?? []);
  readonly totalCount = computed(() => this.#result()?.totalCount ?? 0);
  readonly totalPages = computed(() => this.#result()?.totalPages ?? 0);
  readonly page = computed(() => this.#query().page);
  readonly pageSize = computed(() => this.#query().pageSize);
  readonly sortBy = computed(() => this.#query().sortBy);
  readonly sortOrder = computed(() => this.#query().sortOrder);
  readonly loadState = this.#loadState.asReadonly();
  readonly error = this.#error.asReadonly();
  readonly refreshMsg = this.#refreshMsg.asReadonly();
  readonly isLoading = computed(() => this.#loadState() === "loading");
  readonly isRefreshing = computed(() => this.#loadState() === "refreshing");

  readonly maxTagPct = computed(() => {
    const items = this.tags();
    return items.length ? Math.max(...items.map((t) => t.percentage)) : 1;
  });

  // Streams
  readonly #load$ = new Subject<TagsQuery>();
  readonly #refresh$ = new Subject<void>();

  constructor() {
    this.#load$
      .pipe(
        tap(() => {
          this.#loadState.set("loading");
          this.#error.set(null);
        }),
        switchMap((q) => this.#api.getTags(q)),
        takeUntilDestroyed(),
      )
      .subscribe({
        next: (result) => {
          this.#result.set(result);
          this.#loadState.set("idle");
        },
        error: () => {
          this.#error.set("Failed to load tags. Is the backend running?");
          this.#loadState.set("error");
        },
      });

    this.#refresh$
      .pipe(
        tap(() => {
          this.#loadState.set("refreshing");
          this.#refreshMsg.set(null);
        }),
        switchMap(() => this.#api.refreshTags()),
        takeUntilDestroyed(),
      )
      .subscribe({
        next: (res: RefreshResult) => {
          this.#refreshMsg.set(`✓ Refreshed ${res.fetchedCount} tags`);
          this.#loadState.set("idle");
          this.#load$.next(this.#query());
        },
        error: () => {
          this.#refreshMsg.set("Refresh failed — check backend connection.");
          this.#loadState.set("idle");
        },
      });

    // Load on init
    this.load();
  }

  // Actions

  load(): void {
    this.#load$.next(this.#query());
  }

  setPage(page: number): void {
    this.#query.update((q) => ({ ...q, page }));
    this.load();
  }

  setPageSize(pageSize: number): void {
    this.#query.update((q) => ({ ...q, pageSize, page: 1 }));
    this.load();
  }

  toggleSort(field: SortField): void {
    this.#query.update((q) => ({
      ...q,
      sortBy: field,
      sortOrder:
        q.sortBy === field ? (q.sortOrder === "asc" ? "desc" : "asc") : "asc",
      page: 1,
    }));
    this.load();
  }

  refresh(): void {
    this.#refresh$.next();
  }
}
