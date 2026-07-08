function App() {
	function handleNavClick(e) {
		setActiveCategory(e.target.innerHTML);
	}

	var [activeCategory, setActiveCategory] = React.useState("top");
	return (<Nav navItems={settings_nav_items} activeCategory={activeCategory} clickHandler={handleNavClick} />);
}