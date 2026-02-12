import { useQuery } from '@tanstack/react-query';
import { categoryService } from '../../api/services';
import { Link } from 'react-router-dom';

const CatalogPage = () => {
    const { data: categories, isLoading, error } = useQuery({
        queryKey: ['categories'],
        queryFn: categoryService.getAll,
    });

    if (isLoading) return <div>Ładowanie kategorii...</div>;
    if (error) return <div>Wystąpił błąd podczas pobierania kategorii.</div>;

    return (
        <div className="catalog-container">
            <h1>Katalog Nauki</h1>
            <div className="category-grid">
                {categories?.map((category) => (
                    <div key={category.id} className="category-card">
                        <h3>{category.name}</h3>
                        <p>{category.description}</p>
                        <Link to={`/catalog/${category.id}`} className="button-link">Zobacz zestawy</Link>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CatalogPage;
