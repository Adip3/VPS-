import CrudPage from '../../components/CrudPage'

export function StaffPage() {
  return <CrudPage
    title="Staff Management"
    resource="staff"
    columns={[
      { key: 'fullName', label: 'Full Name' },
      { key: 'email', label: 'Email' },
      { key: 'phone', label: 'Phone' },
      { key: 'position', label: 'Position' },
      { key: 'salary', label: 'Salary' }
    ]}
    formFields={[
      { key: 'email', label: 'Email', required: true },
      { key: 'password', label: 'Password (only on add)' },
      { key: 'fullName', label: 'Full Name', required: true },
      { key: 'phone', label: 'Phone' },
      { key: 'position', label: 'Position' },
      { key: 'salary', label: 'Salary', type: 'number' },
      { key: 'isActive', label: 'Active (true/false)' }
    ]} />
}

export function VendorsPage() {
  return <CrudPage
    title="Vendors"
    resource="vendors"
    columns={[
      { key: 'name', label: 'Name' },
      { key: 'contact', label: 'Contact' },
      { key: 'email', label: 'Email' },
      { key: 'address', label: 'Address' }
    ]}
    formFields={[
      { key: 'name', label: 'Name', required: true },
      { key: 'contact', label: 'Contact' },
      { key: 'email', label: 'Email' },
      { key: 'address', label: 'Address' }
    ]} />
}

export function PartsPage() {
  return <CrudPage
    title="Parts Catalog"
    resource="parts"
    columns={[
      { key: 'name', label: 'Name' },
      { key: 'sku', label: 'SKU' },
      { key: 'categoryName', label: 'Category' },
      { key: 'vendorName', label: 'Vendor' },
      { key: 'buyPrice', label: 'Buy' },
      { key: 'sellPrice', label: 'Sell' },
      { key: 'stock', label: 'Stock' }
    ]}
    formFields={[
      { key: 'name', label: 'Name', required: true },
      { key: 'sku', label: 'SKU', required: true },
      { key: 'categoryId', label: 'Category Id', type: 'number' },
      { key: 'vendorId', label: 'Vendor Id', type: 'number' },
      { key: 'buyPrice', label: 'Buy Price', type: 'number', required: true },
      { key: 'sellPrice', label: 'Sell Price', type: 'number', required: true },
      { key: 'stock', label: 'Stock', type: 'number', required: true },
      { key: 'avgLifespanKm', label: 'Avg Lifespan (km)', type: 'number' }
    ]} />
}
