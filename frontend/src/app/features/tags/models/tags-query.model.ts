import { SortField } from "./sort-field.model";
import { SortOrder } from "./sort-order.model";

export interface TagsQuery {
  page: number;
  pageSize: number;
  sortBy: SortField;
  sortOrder: SortOrder;
}
