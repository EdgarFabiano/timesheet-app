import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-skeleton',
  standalone: true,
  imports: [CommonModule],
  template: `
    @if (type === 'table') {
      <div class="skeleton-table-wrapper">
        <table class="skeleton-table">
          <thead>
            <tr>
              @for (col of columns; track col) {
                <th>
                  <div class="skeleton-cell skeleton-header-cell"></div>
                </th>
              }
            </tr>
          </thead>
          <tbody>
            @for (row of rows; track $index) {
              <tr>
                @for (col of columns; track col) {
                  <td>
                    <div class="skeleton-cell" [style.width]="getCellWidth($index)"></div>
                  </td>
                }
              </tr>
            }
          </tbody>
        </table>
      </div>
    } @else if (type === 'card') {
      <div class="skeleton-cards">
        @for (card of rows; track $index) {
          <div class="skeleton-card">
            <div class="skeleton-line skeleton-title"></div>
            <div class="skeleton-line"></div>
            <div class="skeleton-line skeleton-short"></div>
          </div>
        }
      </div>
    } @else if (type === 'chart') {
      <div class="skeleton-chart">
        <div class="skeleton-line skeleton-title"></div>
        <div class="skeleton-chart-area"></div>
      </div>
    } @else if (type === 'list') {
      <div class="skeleton-list">
        @for (item of rows; track $index) {
          <div class="skeleton-list-item">
            <div class="skeleton-avatar"></div>
            <div class="skeleton-list-content">
              <div class="skeleton-line skeleton-medium"></div>
              <div class="skeleton-line skeleton-short"></div>
            </div>
          </div>
        }
      </div>
    } @else {
      <div class="skeleton-text-lines">
        @for (line of rows; track $index) {
          <div class="skeleton-line" [style.width]="getLineWidth($index)"></div>
        }
      </div>
    }
  `,
  styles: [`
    .skeleton-table-wrapper {
      width: 100%;
      overflow-x: auto;
      min-height: 300px;
    }

    .skeleton-table {
      width: 100%;
      border-collapse: collapse;
    }

    .skeleton-table th,
    .skeleton-table td {
      padding: 12px 16px;
      text-align: left;
      border-bottom: 1px solid var(--mat-sys-outline-variant, #e0e0e0);
    }

    .skeleton-table th {
      background: var(--mat-sys-surface-container-highest, #eeeeee);
      font-weight: 500;
    }

    .skeleton-table tbody tr:hover {
      background: transparent;
    }

    .skeleton-header-cell {
      height: 20px;
      width: 100px;
      border-radius: 4px;
    }

    .skeleton-cell {
      height: 16px;
      border-radius: 4px;
    }

    .skeleton-cards {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
      gap: 24px;
    }

    .skeleton-card {
      padding: 24px;
      border-radius: 12px;
      background: var(--mat-sys-surface-container-low, #f5f5f5);
    }

    .skeleton-chart {
      padding: 24px;
      border-radius: 12px;
      background: var(--mat-sys-surface-container-low, #f5f5f5);
    }

    .skeleton-chart-area {
      height: 200px;
      margin-top: 16px;
      border-radius: 8px;
    }

    .skeleton-list {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .skeleton-list-item {
      display: flex;
      align-items: center;
      gap: 16px;
      padding: 12px;
    }

    .skeleton-avatar {
      width: 40px;
      height: 40px;
      border-radius: 50%;
      flex-shrink: 0;
    }

    .skeleton-list-content {
      flex: 1;
      display: flex;
      flex-direction: column;
      gap: 8px;
    }

    .skeleton-text-lines {
      display: flex;
      flex-direction: column;
      gap: 12px;
    }

    .skeleton-line {
      height: 16px;
      border-radius: 4px;
    }

    .skeleton-title {
      height: 24px;
      width: 60%;
      margin-bottom: 8px;
    }

    .skeleton-short {
      width: 40%;
    }

    .skeleton-medium {
      width: 70%;
    }

    .skeleton-cell,
    .skeleton-line,
    .skeleton-header-cell,
    .skeleton-avatar,
    .skeleton-chart-area {
      background: linear-gradient(
        90deg,
        var(--mat-sys-surface-container-low, #f0f0f0) 25%,
        var(--mat-sys-surface-container-highest, #e0e0e0) 50%,
        var(--mat-sys-surface-container-low, #f0f0f0) 75%
      );
      background-size: 200% 100%;
      animation: skeleton-shimmer 1.5s ease-in-out infinite;
    }

    @keyframes skeleton-shimmer {
      0% {
        background-position: 200% 0;
      }
      100% {
        background-position: -200% 0;
      }
    }
  `]
})
export class SkeletonComponent {
  @Input() type: 'table' | 'card' | 'chart' | 'list' | 'text' = 'text';
  @Input() count = 5;

  get rows(): number[] {
    return Array(this.count).fill(0).map((_, i) => i);
  }

  get columns(): number[] {
    return [1, 2, 3, 4, 5];
  }

  getCellWidth(index: number): string {
    const widths = ['80%', '60%', '70%', '50%', '40%'];
    return widths[index % widths.length];
  }

  getLineWidth(index: number): string {
    const widths = ['100%', '85%', '90%', '75%', '80%'];
    return widths[index % widths.length];
  }
}
