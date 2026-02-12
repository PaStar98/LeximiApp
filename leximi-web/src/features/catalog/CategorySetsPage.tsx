import { useParams, Link } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { learningSetService } from '../../api/services';

const CategorySetsPage = () => {
    const { categoryId } = useParams<{ categoryId: string }>();

    const { data: sets, isLoading, error } = useQuery({
        queryKey: ['sets', categoryId],
        queryFn: () => learningSetService.getByCategory(categoryId!),
        enabled: !!categoryId,
    });

    if (isLoading) return <div>Ładowanie zestawów...</div>;
    if (error) return <div>Wystąpił błąd podczas pobierania zestawów.</div>;
    if (!sets || sets.length === 0) return <div>Brak zestawów w tej kategorii.</div>;

    return (
        <div className="sets-container">
            <h2>Zestawy do nauki</h2>
            <div className="sets-list">
                {sets.map((set) => (
                    <div key={set.id} className="set-card">
                        <h3>{set.title}</h3>
                        <p>{set.description}</p>
                        <span className="set-type-badge">{set.type}</span>
                        <Link to={`/sets/${set.id}`} className="button-link">Wybierz</Link>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default CategorySetsPage;
