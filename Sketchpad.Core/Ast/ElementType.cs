namespace Sketchpad.Core.Ast;

public enum ElementType
{
    Unknown,

    // Layout
    Window, Panel, Card, Row, Col, Divider, Spacer,

    // Navigation
    Navbar, Sidebar, Menu, Nav, Item, Tabs, Tab, Brand,

    // Form
    Field, Textarea, Checkbox, Radio, Select, Toggle, Slider, Button,

    // Display
    Label, Text, Heading, Avatar, Image, Badge, Tag, Table, Columns, Icon,

    // Feedback
    Alert, Toast, Spinner, Progress,

    // Date / Time
    DatePicker, DateTimePicker, Calendar,
}
