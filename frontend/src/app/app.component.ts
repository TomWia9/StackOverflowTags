import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TagsTableComponent } from './features/tags/components/tags-table.component';

@Component({
  selector: 'app-root',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TagsTableComponent],
  template: `<app-tags-table />`,
})
export class AppComponent {}
