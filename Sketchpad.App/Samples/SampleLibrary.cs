namespace Sketchpad.App.Samples;

public record Sample(string Name, string Description, string Dsl);

public static class SampleLibrary
{
    // ── Item View ────────────────────────────────────────────────────────────

    public static readonly Sample ItemView = new(
        "Item View",
        "Read-only display of a single record with sidebar, badges and a detail table.",
        """
        # Contact Detail — read-only item view
        window "Contact Detail" [980x680]

          navbar
            brand "CRM"
            menu [right]
              item "Edit"
              item "Delete"

          row
            sidebar [240px]
              avatar [circle]
              heading "Sarah Mitchell"
              label "Senior Engineer" [muted]
              label "Acme Corp" [muted]
              divider
              label "Tags" [muted]
              row
                badge "VIP"
                badge "Technical"
              divider
              label "Created" [muted]
              label "15 Jan 2024"
              label "Last contact" [muted]
              label "10 Mar 2024"

            col
              card "Contact Information"
                row
                  col
                    label "Email" [muted]
                    label "sarah@acmecorp.com"
                  col
                    label "Phone" [muted]
                    label "+1 (555) 234-5678"
                  col
                    label "Location" [muted]
                    label "San Francisco, CA"
                row
                  col
                    label "Department" [muted]
                    label "Engineering"
                  col
                    label "Reports To" [muted]
                    label "James Carter"
                  col
                    label "Status" [muted]
                    badge "Active"

              card "Recent Activity"
                table
                  columns "Date | Activity | Notes"
                  row "2024-03-10 | Meeting | Discussed Q2 roadmap"
                  row "2024-02-28 | Email | Sent proposal"
                  row "2024-02-15 | Call | Product demo"
                  row "2024-01-30 | Meeting | Onboarding session"
        """);

    // ── Item Edit ────────────────────────────────────────────────────────────

    public static readonly Sample ItemEdit = new(
        "Item Edit",
        "Multi-section editable form for a single record with grouped cards.",
        """
        # Contact Edit — editable item form
        window "Edit Contact" [980x740]

          navbar
            brand "CRM"
            menu [right]
              item "Cancel"

          panel
            card "Personal Information"
              row
                field "First name" = "Sarah"
                field "Last name" = "Mitchell"
                field "Title" = "Senior Engineer"
              row
                field "Email" = "sarah@acmecorp.com" [wide]
              row
                field "Phone" = "+1 (555) 234-5678"
                select "Phone type" = "Work"

            card "Organisation"
              row
                field "Company" = "Acme Corp"
                field "Department" = "Engineering"
              row
                field "Reports to" = "James Carter"
                select "Status" = "Active"

            card "Address"
              row
                field "Street" = "742 Market Street" [wide]
              row
                field "City" = "San Francisco"
                field "State" = "CA"
                field "Postcode" = "94105"
              row
                select "Country" = "United States"

            row
              button "Save changes" [primary]
              button "Cancel"
              button "Delete contact" [danger]
        """);

    // ── List View ─────────────────────────────────────────────────────────────

    public static readonly Sample ListView = new(
        "List View",
        "Searchable, filterable read-only data table with pagination hint.",
        """
        # Contacts List — searchable read-only list
        window "Contacts" [1100x700]

          navbar
            brand "CRM"
            menu [right]
              item "Import"
              item "Export"
              item "New Contact"

          panel
            row
              heading "All Contacts"
              button "New Contact" [primary]
            row
              field "Search" = "Search contacts..."
              select "Company" = "All companies"
              select "Status" = "All statuses"
            table
              columns "Name | Email | Company | Phone | Status | Added"
              row "Sarah Mitchell | sarah@acmecorp.com | Acme Corp | +1 555 234-5678 | Active | 15 Jan 2024"
              row "James Carter | james@techco.io | TechCo | +1 555 876-1234 | Active | 1 Feb 2024"
              row "Priya Sharma | priya@startxyz.com | StartXYZ | +1 555 432-9876 | Lead | 14 Feb 2024"
              row "Nikolai Voronov | n.voronov@bigcorp.eu | BigCorp EU | +44 20 1234 5678 | Inactive | 3 Mar 2024"
              row "Mei-Ling Wu | mei@futurecorp.hk | FutureCorp | +852 9876 5432 | Active | 12 Mar 2024"
            row
              label "Showing 5 of 248 contacts" [muted]
              button "Previous" [disabled]
              button "Next"
        """);

    // ── List Edit ─────────────────────────────────────────────────────────────

    public static readonly Sample ListEdit = new(
        "List Edit",
        "Master/detail: list on the left, edit form on the right — typical CRUD layout.",
        """
        # Product Catalogue — master / detail list edit
        window "Products" [1140x720]

          navbar
            brand "Inventory"
            menu [right]
              item "New Product"
              item "Import CSV"

          panel
            row
              col
                card "Product List"
                  row
                    field "Search" = "Filter products..."
                    select "Category" = "All"
                  table
                    columns "SKU | Name | Price | Stock"
                    row "PRD-001 | Wireless Headphones | $129.99 | 48"
                    row "PRD-002 | Laptop Stand | $49.99 | 112"
                    row "PRD-003 | USB-C Hub | $79.99 | 23"
                    row "PRD-004 | Monitor Arm | $89.99 | 7"
                  label "4 of 38 products" [muted]

              col
                card "Edit Product"
                  heading "Wireless Headphones"
                  field "SKU" = "PRD-001"
                  field "Name" = "Wireless Headphones"
                  select "Category" = "Electronics"
                  row
                    field "Price" = "$129.99"
                    field "Stock qty" = "48"
                  textarea "Description" = "Premium wireless headphones with active noise cancellation and 30-hour battery."
                  toggle "Active listing"
                  divider
                  row
                    button "Save" [primary]
                    button "Delete" [danger]
        """);

    // ── Parent + Child Grid ───────────────────────────────────────────────────

    public static readonly Sample ParentChildGrid = new(
        "Parent + Child Grid",
        "Invoice header with line-items table and a running total — parent record drives a child grid.",
        """
        # Invoice — parent record with child line-items grid
        window "Invoice INV-2024-0089" [1020x820]

          navbar
            brand "Billing"
            menu [right]
              item "Print"
              item "Export PDF"
              item "Send"

          panel
            card "Invoice"
              row
                col
                  label "From" [muted]
                  label "Acme Services Pty Ltd"
                  label "42 Business Rd, Sydney NSW 2000"
                  label "ABN 12 345 678 901"
                col
                  label "Invoice number" [muted]
                  heading "INV-2024-0089"
                  label "Issue date" [muted]
                  label "15 March 2024"
                  label "Due date" [muted]
                  label "14 April 2024"
              divider
              col
                label "Bill to" [muted]
                label "TechCo Pty Ltd — James Carter"
                label "100 Tech Ave, Melbourne VIC 3000"

            card "Line Items"
              table
                columns "Description | Qty | Unit Price | Amount"
                row "Strategy Consulting — March | 15 hrs | $200.00 | $3,000.00"
                row "UX Design Sprint | 1 sprint | $4,500.00 | $4,500.00"
                row "Development — API Integration | 20 hrs | $180.00 | $3,600.00"
                row "Project Management | 8 hrs | $150.00 | $1,200.00"
              divider
              row
                spacer
                col
                  row
                    label "Subtotal" [muted]
                    label "$12,300.00"
                  row
                    label "GST (10%)" [muted]
                    label "$1,230.00"
                  divider
                  row
                    label "Total due"
                    heading "$13,530.00"

            row
              button "Mark as paid" [primary]
              button "Send reminder"
              button "Void invoice" [danger]
        """);

    // ── Multi-column Form ─────────────────────────────────────────────────────

    public static readonly Sample MultiColumnForm = new(
        "Multi-column Form",
        "Settings page mixing labels-on-top fields, labels-to-the-left controls, and multi-column rows.",
        """
        # Account Settings — labels on top, labels to left, multi-column
        window "Account Settings" [1020x800]

          navbar
            brand "MyApp"
            menu [right]
              item "Log out"

          row
            sidebar [200px]
              nav
                item "Profile" [active]
                item "Security"
                item "Notifications"
                item "Billing"
                item "Integrations"

            panel
              card "Personal Information"
                # Labels on top — standard multi-column field layout
                row
                  field "First name" = "Steven"
                  field "Last name" = "Oberholzer"
                  field "Display name" = "steve_o"
                row
                  field "Email address" = "steve@example.com" [wide]
                row
                  select "Time zone" = "Australia/Sydney (UTC+11)"
                  select "Language" = "English (AU)"
                  select "Date format" = "DD/MM/YYYY"

              card "Preferences"
                # Labels to the left — two-column label + control rows
                row
                  label "Theme"
                  select = "System default"
                row
                  label "Density"
                  select = "Comfortable"
                row
                  label "Compact mode"
                  toggle "Enable compact UI"
                row
                  label "Email digests"
                  select = "Weekly summary"
                row
                  label "Marketing emails"
                  checkbox "Send me product updates and announcements"

              card "Security"
                row
                  col
                    label "Password" [muted]
                    label "Last changed 45 days ago"
                  col
                    label "Two-factor auth" [muted]
                    badge "Enabled"
                  col
                    label "Active sessions" [muted]
                    label "3 devices"
                row
                  button "Change password"
                  button "Manage sessions"

              card "Danger Zone" [warning]
                row
                  col
                    label "Delete account"
                    text "Permanently remove your account and all associated data. This cannot be undone."
                  button "Delete account" [danger]

              row
                button "Save changes" [primary]
                button "Cancel"
        """);

    // ── Appointment Booking ───────────────────────────────────────────────────

    public static readonly Sample AppointmentBooking = new(
        "Appointment Booking",
        "Calendar + date/time pickers: side-by-side month view with a booking detail form.",
        """
        # Appointment Booking — calendar beside a date/time form
        window "Book Appointment" [1020x700]

          navbar
            brand "ClinicPlus"
            menu [right]
              item "My Appointments"
              item "Log out"

          row
            col
              card "Select a Date"
                calendar "Available Dates"
                label "Tap a highlighted date to select" [muted]

            col
              card "Appointment Details"
                field "Patient name" = "Jane Smith"
                select "Appointment type" = "General Consultation"
                divider
                datepicker "Date" = "2024-04-15"
                row
                  datetimepicker "Start time" = "2024-04-15 09:00"
                  datetimepicker "End time" = "2024-04-15 09:30"
                divider
                select "Doctor" = "Dr. Sarah Mitchell"
                select "Location" = "Room 4 — Ground Floor"
                textarea "Notes" = "Follow-up from previous visit. Patient requested morning slot."
                divider
                row
                  button "Confirm booking" [primary]
                  button "Cancel"
        """);

    // ── Index (declared last so all fields above are already initialised) ─────

    public static readonly IReadOnlyList<Sample> All =
    [
        ItemView,
        ItemEdit,
        ListView,
        ListEdit,
        ParentChildGrid,
        MultiColumnForm,
        AppointmentBooking,
    ];
}
