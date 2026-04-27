import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { DecimalPipe } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { TagsStore } from "../store/tags.store";
import { SortField } from "../models/sort-field.model";
import { environment } from "src/environments/environment";

@Component({
  selector: "app-tags-table",
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [DecimalPipe, FormsModule],
  templateUrl: "./tags-table.component.html",
  styleUrls: ["./tags-table.component.css"],
})
export class TagsTableComponent {
  readonly store = inject(TagsStore);

  readonly pageSizeOptions = [10, 25, 50, 100];
  readonly docsUrl = `${environment.apiUrl}/scalar/v1`;

  sort(field: SortField): void {
    this.store.toggleSort(field);
  }

  sortIcon(field: SortField): string {
    if (this.store.sortBy() !== field) return "↕";
    return this.store.sortOrder() === "asc" ? "↑" : "↓";
  }

  isSortActive(field: SortField): boolean {
    return this.store.sortBy() === field;
  }

  visiblePages(): number[] {
    const total = this.store.totalPages();
    const current = this.store.page();
    const delta = 2;
    const pages: number[] = [];

    for (
      let i = Math.max(1, current - delta);
      i <= Math.min(total, current + delta);
      i++
    ) {
      pages.push(i);
    }

    // Prepend
    if (pages[0] > 2) pages.unshift(-1, 1);
    else if (pages[0] === 2) pages.unshift(1);

    // Append
    if (pages[pages.length - 1] < total - 1) pages.push(-2, total);
    else if (pages[pages.length - 1] === total - 1) pages.push(total);

    return pages;
  }

  rowRank(index: number): number {
    return (this.store.page() - 1) * this.store.pageSize() + index + 1;
  }
}
