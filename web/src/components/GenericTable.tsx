import type { KeyboardEvent, ReactNode } from 'react';

export type GenericTableProps<T> = {
  items: T[];
  header: ReactNode;
  renderRow: (item: T) => ReactNode;
  onRowClick?: (item: T) => void;
  getRowKey: (item: T) => string;
  emptyText?: string;
  loading?: boolean;
  className?: string;
};

export function GenericTable<T>({
  items,
  header,
  renderRow,
  onRowClick,
  getRowKey,
  emptyText = 'Sin datos',
  loading = false,
  className,
}: GenericTableProps<T>) {
  if (loading) {
    return <div className="state">Cargando…</div>;
  }

  if (items.length === 0) {
    return <div className="state empty">{emptyText}</div>;
  }

  const handleKeyDown = (event: KeyboardEvent<HTMLTableRowElement>, item: T) => {
    if (!onRowClick) return;
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      onRowClick(item);
    }
  };

  const tableClass = ['generic-table', className].filter(Boolean).join(' ');

  return (
    <table className={tableClass}>
      <thead>{header}</thead>
      <tbody>
        {items.map((item) => (
          <tr
            key={getRowKey(item)}
            tabIndex={onRowClick ? 0 : undefined}
            role={onRowClick ? 'button' : undefined}
            onClick={onRowClick ? () => onRowClick(item) : undefined}
            onKeyDown={onRowClick ? (e) => handleKeyDown(e, item) : undefined}
          >
            {renderRow(item)}
          </tr>
        ))}
      </tbody>
    </table>
  );
}
