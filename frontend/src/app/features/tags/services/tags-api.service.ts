import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../../../environments/environment";
import { PaginatedResult } from "../models/paginated-result.model";
import { RefreshResult } from "../models/refresh-result.model";
import { Tag } from "../models/tag.model";
import { TagsQuery } from "../models/tags-query.model";

@Injectable({ providedIn: "root" })
export class TagsApiService {
  readonly #http = inject(HttpClient);
  readonly #base = environment.apiUrl;

  getTags(query: TagsQuery): Observable<PaginatedResult<Tag>> {
    const params = new HttpParams()
      .set("page", query.page)
      .set("pageSize", query.pageSize)
      .set("sortBy", query.sortBy)
      .set("sortOrder", query.sortOrder);

    return this.#http.get<PaginatedResult<Tag>>(`${this.#base}/api/tags`, {
      params,
    });
  }

  refreshTags(): Observable<RefreshResult> {
    return this.#http.post<RefreshResult>(
      `${this.#base}/api/tags/refresh`,
      null,
    );
  }
}
