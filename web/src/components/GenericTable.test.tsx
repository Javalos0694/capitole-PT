import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { GenericTable } from './GenericTable';

type Item = { id: string; name: string };

describe('GenericTable', () => {
  it('renders header and rows', () => {
    render(
      <GenericTable
        items={[{ id: '1', name: 'A' }]}
        getRowKey={(i) => i.id}
        header={
          <tr>
            <th>Name</th>
          </tr>
        }
        renderRow={(item) => <td>{item.name}</td>}
      />,
    );

    expect(screen.getByText('Name')).toBeInTheDocument();
    expect(screen.getByText('A')).toBeInTheDocument();
  });

  it('shows empty text', () => {
    render(
      <GenericTable
        items={[]}
        emptyText="Vacío"
        getRowKey={(i: Item) => i.id}
        header={<tr><th>Name</th></tr>}
        renderRow={() => null}
      />,
    );

    expect(screen.getByText('Vacío')).toBeInTheDocument();
  });

  it('emits item on click and Enter', async () => {
    const user = userEvent.setup();
    const onRowClick = vi.fn();
    const item = { id: '1', name: 'A' };

    render(
      <GenericTable
        items={[item]}
        getRowKey={(i) => i.id}
        onRowClick={onRowClick}
        header={
          <tr>
            <th>Name</th>
          </tr>
        }
        renderRow={(row) => <td>{row.name}</td>}
      />,
    );

    await user.click(screen.getByText('A'));
    expect(onRowClick).toHaveBeenCalledWith(item);

    const row = screen.getByRole('button');
    row.focus();
    await user.keyboard('{Enter}');
    expect(onRowClick).toHaveBeenCalledTimes(2);
  });
});
